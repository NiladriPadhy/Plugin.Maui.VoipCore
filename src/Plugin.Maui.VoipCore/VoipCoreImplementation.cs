#if ANDROID || IOS
using Microsoft.Maui.ApplicationModel;
#endif

namespace Plugin.Maui.VoipCore;

sealed class VoipCoreImplementation : IVoipCore, ISipStackSink, IVoipCallUiHandler, IDisposable
{
    readonly VoipCoreOptions _options;
    readonly ISipStack _stack;
    readonly IVoipAudioSession _audio;
    readonly IVoipCallUi _callUi;
    readonly IClock _clock;
    readonly object _sync = new();
    readonly Dictionary<string, VoipCall> _calls = new(StringComparer.Ordinal);
    readonly HashSet<string> _heldOnBackground = new(StringComparer.Ordinal);
    readonly SemaphoreSlim _op = new(1, 1);
    readonly VoipAudio _audioFacade;

    bool _initialized;
    bool _disposed;

    public VoipCoreImplementation(
        VoipCoreOptions options,
        ISipStack stack,
        IVoipAudioSession audio,
        IVoipCallUi callUi,
        IClock clock)
    {
        _options = options;
        _stack = stack;
        _audio = audio;
        _callUi = callUi;
        _clock = clock;
        _audioFacade = new VoipAudio(audio, RaiseAudioRouteChanged);
        Options = options;
        Stack = stack;
        Audio = _audioFacade;
    }

    public bool IsSupported => true;

    public bool IsInitialized
    {
        get
        {
            lock (_sync)
            {
                return _initialized;
            }
        }
    }

    public VoipEngineState State { get; private set; } = VoipEngineState.Idle;

    public RegistrationState Registration { get; private set; } = RegistrationState.None;

    public SipAccount? Account { get; private set; }

    public ISipStack Stack { get; }

    public IVoipAudio Audio { get; }

    public VoipCoreOptions Options { get; }

    public IReadOnlyList<IVoipCall> Calls
    {
        get
        {
            lock (_sync)
            {
                return _calls.Values.ToArray();
            }
        }
    }

    public IVoipCall? ActiveCall
    {
        get
        {
            lock (_sync)
            {
                return SelectActive(_calls.Values);
            }
        }
    }

    public event EventHandler<RegistrationChangedEventArgs>? RegistrationChanged;

    public event EventHandler<CallChangedEventArgs>? CallChanged;

    public event EventHandler<IncomingCallEventArgs>? IncomingCall;

    public event EventHandler<AudioRouteChangedEventArgs>? AudioRouteChanged;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _op.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();
            if (_initialized)
            {
                return;
            }

            await _stack.InitializeAsync(new SipStackContext { Sink = this, Options = _options }, cancellationToken)
                .ConfigureAwait(false);
            _callUi.Attach(this);

            lock (_sync)
            {
                _initialized = true;
                State = VoipEngineState.Ready;
            }

            if (_options.AutoRegister && _options.Account is not null)
            {
                await RegisterCoreAsync(_options.Account, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (VoipCoreException)
        {
            lock (_sync)
            {
                State = VoipEngineState.Failed;
            }

            throw;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            lock (_sync)
            {
                State = VoipEngineState.Failed;
            }

            throw new VoipCoreException(VoipCoreError.StackFailure, "The SIP stack failed to initialize.", ex);
        }
        finally
        {
            _op.Release();
        }
    }

    public async Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        await _op.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!_initialized)
            {
                return;
            }

            await HangupAllCoreAsync(CallEndReason.Shutdown, cancellationToken).ConfigureAwait(false);

            if (Registration is RegistrationState.Registered or RegistrationState.Registering)
            {
                try
                {
                    await _stack.UnregisterAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // Best-effort during shutdown.
                }
            }

            try
            {
                await _stack.ShutdownAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // Best-effort during shutdown.
            }

            await RestoreAudioQuietlyAsync(cancellationToken).ConfigureAwait(false);

            lock (_sync)
            {
                _initialized = false;
                Account = null;
                State = VoipEngineState.Idle;
                Registration = RegistrationState.None;
                _heldOnBackground.Clear();
            }
        }
        finally
        {
            _op.Release();
        }
    }

    public async Task RegisterAsync(SipAccount account, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(account);
        await _op.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();
            await EnsureInitializedCoreAsync(cancellationToken).ConfigureAwait(false);
            await RegisterCoreAsync(account, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _op.Release();
        }
    }

    public async Task UnregisterAsync(CancellationToken cancellationToken = default)
    {
        await _op.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();
            EnsureInitializedLocked();

            SetEngine(VoipEngineState.Unregistering, RegistrationState.Unregistering);
            try
            {
                await _stack.UnregisterAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException and not VoipCoreException)
            {
                throw new VoipCoreException(VoipCoreError.StackFailure, "Unregister failed.", ex);
            }

            lock (_sync)
            {
                if (Registration != RegistrationState.Unregistered)
                {
                    Registration = RegistrationState.Unregistered;
                }

                State = VoipEngineState.Ready;
            }

            RaiseRegistration(Registration, null);
        }
        finally
        {
            _op.Release();
        }
    }

    public async Task<IVoipCall> PlaceCallAsync(CallRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var normalized = request.Normalize();

        await _op.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();
            EnsureInitializedLocked();
            EnsureRegistered();
            EnsureCallCapacity();

            string callId;
            try
            {
                callId = await _stack.PlaceCallAsync(normalized, cancellationToken).ConfigureAwait(false);
            }
            catch (VoipCoreException)
            {
                throw;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new VoipCoreException(VoipCoreError.CallFailed, "The SIP stack could not place the call.", ex);
            }

            if (string.IsNullOrWhiteSpace(callId))
            {
                throw new VoipCoreException(VoipCoreError.CallFailed, "The SIP stack returned an empty call id.");
            }

            var call = GetOrAddCall(
                callId,
                CallDirection.Outgoing,
                normalized.Destination,
                normalized.DisplayName,
                normalized.Video);

            await ConfigureAudioAsync(cancellationToken).ConfigureAwait(false);
            await SafeUiAsync(() => _callUi.ReportOutgoingAsync(call, cancellationToken)).ConfigureAwait(false);
            RaiseCallChanged(call);
            return call;
        }
        finally
        {
            _op.Release();
        }
    }

    public async Task<IVoipCall> AnswerAsync(string callId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(callId);
        await _op.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();
            EnsureInitializedLocked();
            var call = RequireCall(callId);
            if (call.Direction != CallDirection.Incoming || call.IsTerminal)
            {
                throw new VoipCoreException(VoipCoreError.InvalidOperation, $"Call '{callId}' cannot be answered.");
            }

            try
            {
                await _stack.AnswerAsync(callId, cancellationToken).ConfigureAwait(false);
            }
            catch (VoipCoreException)
            {
                throw;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new VoipCoreException(VoipCoreError.CallFailed, "The SIP stack could not answer the call.", ex);
            }

            await ConfigureAudioAsync(cancellationToken).ConfigureAwait(false);
            return RequireCall(callId);
        }
        finally
        {
            _op.Release();
        }
    }

    public async Task RejectAsync(string callId, CallRejectReason reason = CallRejectReason.Decline, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(callId);
        await _op.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();
            EnsureInitializedLocked();
            _ = RequireCall(callId);
            await _stack.RejectAsync(callId, reason, cancellationToken).ConfigureAwait(false);
        }
        catch (VoipCoreException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new VoipCoreException(VoipCoreError.CallFailed, "The SIP stack could not reject the call.", ex);
        }
        finally
        {
            _op.Release();
        }
    }

    public async Task HangupAsync(string callId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(callId);
        await _op.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();
            EnsureInitializedLocked();
            var call = RequireCall(callId);
            if (call.IsTerminal)
            {
                return;
            }

            await _stack.HangupAsync(callId, cancellationToken).ConfigureAwait(false);
        }
        catch (VoipCoreException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new VoipCoreException(VoipCoreError.CallFailed, "The SIP stack could not hang up the call.", ex);
        }
        finally
        {
            _op.Release();
        }
    }

    public async Task HangupAllAsync(CancellationToken cancellationToken = default)
    {
        await _op.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();
            await HangupAllCoreAsync(CallEndReason.LocalHangup, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _op.Release();
        }
    }

    public async Task SetMutedAsync(string callId, bool muted, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(callId);
        await _op.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();
            var call = RequireCall(callId);
            await _stack.SetMutedAsync(callId, muted, cancellationToken).ConfigureAwait(false);
            call.SetMuted(muted);
            RaiseCallChanged(call);
        }
        finally
        {
            _op.Release();
        }
    }

    public async Task SetHeldAsync(string callId, bool held, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(callId);
        await _op.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();
            _ = RequireCall(callId);
            await _stack.SetHeldAsync(callId, held, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _op.Release();
        }
    }

    public async Task SendDtmfAsync(string callId, string digits, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(callId);
        if (!SipUri.IsValidDtmf(digits))
        {
            throw new VoipCoreException(VoipCoreError.InvalidDtmf, "DTMF digits must be 0-9, *, #, or A-D.");
        }

        await _op.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();
            _ = RequireCall(callId);
            await _stack.SendDtmfAsync(callId, digits, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _op.Release();
        }
    }

    public async Task TransferAsync(string callId, string destination, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(callId);
        var uri = SipUri.Normalize(destination);
        await _op.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();
            _ = RequireCall(callId);
            await _stack.TransferAsync(callId, uri, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _op.Release();
        }
    }

    public IVoipCall? GetCall(string callId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(callId);
        lock (_sync)
        {
            return _calls.TryGetValue(callId, out var call) ? call : null;
        }
    }

    public async Task<bool> EnsureMicrophoneAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
#if ANDROID || IOS
        var status = await Permissions.CheckStatusAsync<Permissions.Microphone>().ConfigureAwait(false);
        if (status == PermissionStatus.Granted)
        {
            return true;
        }

        status = await Permissions.RequestAsync<Permissions.Microphone>().ConfigureAwait(false);
        return status == PermissionStatus.Granted;
#else
        return true;
#endif
    }

    public void NotifyForeground()
    {
        if (!_options.HoldOnBackground)
        {
            return;
        }

        string[] ids;
        lock (_sync)
        {
            ids = _heldOnBackground.ToArray();
            _heldOnBackground.Clear();
        }

        foreach (var id in ids)
        {
            _ = ResumeQuietlyAsync(id);
        }
    }

    public void NotifyBackground()
    {
        if (_options.UnregisterOnBackground && Registration == RegistrationState.Registered)
        {
            _ = UnregisterQuietlyAsync();
        }

        if (!_options.HoldOnBackground)
        {
            return;
        }

        VoipCall[] connected;
        lock (_sync)
        {
            connected = _calls.Values.Where(c => c.State == CallState.Connected).ToArray();
        }

        foreach (var call in connected)
        {
            lock (_sync)
            {
                _heldOnBackground.Add(call.Id);
            }

            _ = HoldQuietlyAsync(call.Id);
        }
    }

    public void OnRegistrationChanged(RegistrationState state, string? message = null)
    {
        VoipEngineState engine;
        lock (_sync)
        {
            Registration = state;
            engine = state switch
            {
                RegistrationState.Registering => VoipEngineState.Registering,
                RegistrationState.Registered => VoipEngineState.Registered,
                RegistrationState.Unregistering => VoipEngineState.Unregistering,
                RegistrationState.Failed => VoipEngineState.Failed,
                _ => _initialized ? VoipEngineState.Ready : VoipEngineState.Idle
            };
            State = engine;
        }

        RaiseRegistration(state, message);
    }

    public void OnIncomingCall(IncomingCallInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);
        if (string.IsNullOrWhiteSpace(info.CallId))
        {
            return;
        }

        var call = GetOrAddCall(info.CallId, CallDirection.Incoming, info.RemoteUri, info.DisplayName, info.Video);
        _ = SafeUiAsync(() => _callUi.ReportIncomingAsync(call, CancellationToken.None));
        var args = new IncomingCallEventArgs(call);
        IncomingCall?.Invoke(this, args);
        _options.Events.OnIncomingCall?.Invoke(args);
        RaiseCallChanged(call);
    }

    public void OnCallStateChanged(string callId, CallState state, CallEndReason? endReason = null, string? message = null)
    {
        if (string.IsNullOrWhiteSpace(callId))
        {
            return;
        }

        VoipCall? call;
        var becameTerminal = false;
        lock (_sync)
        {
            if (!_calls.TryGetValue(callId, out call))
            {
                return;
            }

            call.ApplyState(state, endReason, message, _clock.UtcNow);
            becameTerminal = call.IsTerminal;
        }

        if (state is CallState.Connected)
        {
            _ = SafeUiAsync(() => _callUi.ReportConnectedAsync(call, CancellationToken.None));
        }

        if (becameTerminal)
        {
            _ = SafeUiAsync(() => _callUi.ReportEndedAsync(call, CancellationToken.None));
            _ = RestoreAudioIfIdleAsync();
        }

        RaiseCallChanged(call);
    }

    public Task AnswerFromSystemAsync(string callId) => AnswerAsync(callId);

    public Task HangupFromSystemAsync(string callId) => HangupAsync(callId);

    public Task SetMutedFromSystemAsync(string callId, bool muted) => SetMutedAsync(callId, muted);

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        try
        {
            ShutdownAsync().GetAwaiter().GetResult();
        }
        catch (Exception)
        {
            // Dispose must not throw.
        }

        _op.Dispose();
    }

    async Task RegisterCoreAsync(SipAccount account, CancellationToken cancellationToken)
    {
        account.Validate();
        SetEngine(VoipEngineState.Registering, RegistrationState.Registering);
        Account = account;

        try
        {
            await _stack.RegisterAsync(account, cancellationToken).ConfigureAwait(false);
        }
        catch (VoipCoreException)
        {
            SetEngine(VoipEngineState.Failed, RegistrationState.Failed);
            throw;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            SetEngine(VoipEngineState.Failed, RegistrationState.Failed);
            throw new VoipCoreException(VoipCoreError.RegistrationFailed, "SIP registration failed.", ex);
        }

        lock (_sync)
        {
            if (Registration != RegistrationState.Registered)
            {
                Registration = RegistrationState.Registered;
                State = VoipEngineState.Registered;
            }
        }

        RaiseRegistration(Registration, null);
    }

    async Task EnsureInitializedCoreAsync(CancellationToken cancellationToken)
    {
        if (_initialized)
        {
            return;
        }

        await _stack.InitializeAsync(new SipStackContext { Sink = this, Options = _options }, cancellationToken)
            .ConfigureAwait(false);
        _callUi.Attach(this);
        lock (_sync)
        {
            _initialized = true;
            if (State == VoipEngineState.Idle)
            {
                State = VoipEngineState.Ready;
            }
        }
    }

    async Task HangupAllCoreAsync(CallEndReason reason, CancellationToken cancellationToken)
    {
        VoipCall[] active;
        lock (_sync)
        {
            active = _calls.Values.Where(c => !c.IsTerminal).ToArray();
        }

        foreach (var call in active)
        {
            try
            {
                await _stack.HangupAsync(call.Id, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                OnCallStateChanged(call.Id, CallState.Disconnected, reason);
            }
        }
    }

    async Task ConfigureAudioAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _audio.ConfigureForCallAsync(cancellationToken).ConfigureAwait(false);
            if (_options.UseSpeakerByDefault)
            {
                await _audio.SetSpeakerAsync(true, cancellationToken).ConfigureAwait(false);
                RaiseAudioRouteChanged(_audio.Route, _audio.IsSpeakerOn);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new VoipCoreException(VoipCoreError.AudioFailed, "Platform audio could not be configured for a call.", ex);
        }
    }

    async Task RestoreAudioIfIdleAsync()
    {
        bool idle;
        lock (_sync)
        {
            idle = _calls.Values.All(c => c.IsTerminal);
        }

        if (idle)
        {
            await RestoreAudioQuietlyAsync(CancellationToken.None).ConfigureAwait(false);
        }
    }

    async Task RestoreAudioQuietlyAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _audio.RestoreAsync(cancellationToken).ConfigureAwait(false);
            RaiseAudioRouteChanged(_audio.Route, _audio.IsSpeakerOn);
        }
        catch (Exception)
        {
            // Audio restore is best-effort.
        }
    }

    async Task HoldQuietlyAsync(string callId)
    {
        try
        {
            await SetHeldAsync(callId, true).ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Background hold is best-effort.
        }
    }

    async Task ResumeQuietlyAsync(string callId)
    {
        try
        {
            await SetHeldAsync(callId, false).ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Foreground resume is best-effort.
        }
    }

    async Task UnregisterQuietlyAsync()
    {
        try
        {
            await UnregisterAsync().ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Background unregister is best-effort.
        }
    }

    static async Task SafeUiAsync(Func<Task> work)
    {
        try
        {
            await work().ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Native call UI is optional.
        }
    }

    VoipCall GetOrAddCall(string callId, CallDirection direction, string remoteUri, string? displayName, bool video)
    {
        lock (_sync)
        {
            if (_calls.TryGetValue(callId, out var existing))
            {
                return existing;
            }

            var call = new VoipCall(callId, direction, remoteUri, displayName, video, _clock);
            _calls[callId] = call;
            return call;
        }
    }

    VoipCall RequireCall(string callId)
    {
        lock (_sync)
        {
            if (_calls.TryGetValue(callId, out var call))
            {
                return call;
            }
        }

        throw new VoipCoreException(VoipCoreError.CallNotFound, $"No call exists with id '{callId}'.");
    }

    void EnsureInitializedLocked()
    {
        lock (_sync)
        {
            if (!_initialized)
            {
                throw new VoipCoreException(VoipCoreError.NotInitialized, "Call InitializeAsync before using VoipCore.");
            }
        }
    }

    void EnsureRegistered()
    {
        if (Registration != RegistrationState.Registered)
        {
            throw new VoipCoreException(VoipCoreError.NotRegistered, "Register a SIP account before placing a call.");
        }
    }

    void EnsureCallCapacity()
    {
        lock (_sync)
        {
            var active = _calls.Values.Count(c => !c.IsTerminal);
            if (active >= _options.MaxConcurrentCalls)
            {
                throw new VoipCoreException(
                    VoipCoreError.CallLimitReached,
                    $"At most {_options.MaxConcurrentCalls} concurrent call(s) are allowed.");
            }
        }
    }

    void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new VoipCoreException(VoipCoreError.Disposed, "This VoipCore instance has been disposed.");
        }
    }

    void SetEngine(VoipEngineState engine, RegistrationState registration)
    {
        lock (_sync)
        {
            State = engine;
            Registration = registration;
        }

        RaiseRegistration(registration, null);
    }

    void RaiseRegistration(RegistrationState state, string? message)
    {
        var args = new RegistrationChangedEventArgs(state, message);
        RegistrationChanged?.Invoke(this, args);
        _options.Events.OnRegistrationChanged?.Invoke(args);
    }

    void RaiseCallChanged(IVoipCall call)
    {
        var args = new CallChangedEventArgs(call);
        CallChanged?.Invoke(this, args);
        _options.Events.OnCallChanged?.Invoke(args);
    }

    void RaiseAudioRouteChanged(AudioRoute route, bool speakerOn)
    {
        var args = new AudioRouteChangedEventArgs(route, speakerOn);
        AudioRouteChanged?.Invoke(this, args);
        _options.Events.OnAudioRouteChanged?.Invoke(args);
    }

    static VoipCall? SelectActive(IEnumerable<VoipCall> calls)
    {
        VoipCall? best = null;
        foreach (var call in calls)
        {
            if (call.IsTerminal)
            {
                continue;
            }

            if (best is null || Rank(call.State) > Rank(best.State))
            {
                best = call;
            }
        }

        return best;
    }

    static int Rank(CallState state) => state switch
    {
        CallState.Connected => 50,
        CallState.Held => 40,
        CallState.Holding => 35,
        CallState.Connecting => 30,
        CallState.EarlyMedia => 25,
        CallState.Ringing => 20,
        CallState.Dialing => 15,
        CallState.Transferring => 10,
        _ => 0
    };
}
