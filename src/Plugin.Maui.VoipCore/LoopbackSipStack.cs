namespace Plugin.Maui.VoipCore;

/// <summary>
/// In-process SIP stack for tests, samples, and UI development. No network I/O.
/// </summary>
public sealed class LoopbackSipStack : ISipStack
{
    readonly LoopbackSipStackOptions _options;
    readonly object _sync = new();
    readonly Dictionary<string, LoopCall> _calls = new(StringComparer.Ordinal);
    readonly List<string> _dtmf = [];
    ISipStackSink? _sink;
    int _sequence;

    /// <summary>
    /// Creates a loopback stack with default options (auto-progress outgoing calls).
    /// </summary>
    public LoopbackSipStack()
        : this(new LoopbackSipStackOptions())
    {
    }

    /// <summary>
    /// Creates a loopback stack with the supplied options.
    /// </summary>
    public LoopbackSipStack(LoopbackSipStackOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public string Name => "loopback";

    /// <summary>
    /// DTMF sequences sent through <see cref="SendDtmfAsync"/>.
    /// </summary>
    public IReadOnlyList<string> SentDtmf
    {
        get
        {
            lock (_sync)
            {
                return _dtmf.ToArray();
            }
        }
    }

    /// <inheritdoc />
    public Task InitializeAsync(SipStackContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        _sink = context.Sink;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ShutdownAsync(CancellationToken cancellationToken)
    {
        lock (_sync)
        {
            _calls.Clear();
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RegisterAsync(SipAccount account, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(account);
        account.Validate();

        if (_options.FailRegistration)
        {
            var message = _options.FailRegistrationMessage ?? "Loopback registration failed.";
            _sink?.OnRegistrationChanged(RegistrationState.Failed, message);
            throw new VoipCoreException(VoipCoreError.RegistrationFailed, message);
        }

        _sink?.OnRegistrationChanged(RegistrationState.Registered);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task UnregisterAsync(CancellationToken cancellationToken)
    {
        _sink?.OnRegistrationChanged(RegistrationState.Unregistered);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<string> PlaceCallAsync(CallRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (_options.FailCalls)
        {
            throw new VoipCoreException(VoipCoreError.CallFailed, "Loopback is configured to fail outgoing calls.");
        }

        var id = NewId();
        lock (_sync)
        {
            _calls[id] = new LoopCall
            {
                Id = id,
                RemoteUri = request.Destination,
                DisplayName = request.DisplayName,
                Direction = CallDirection.Outgoing
            };
        }

        if (_options.AutoProgress)
        {
            _ = ProgressOutgoingAsync(id);
        }

        return Task.FromResult(id);
    }

    /// <inheritdoc />
    public Task AnswerAsync(string callId, CancellationToken cancellationToken)
    {
        GetCall(callId);
        _sink?.OnCallStateChanged(callId, CallState.Connecting);
        _sink?.OnCallStateChanged(callId, CallState.Connected);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RejectAsync(string callId, CallRejectReason reason, CancellationToken cancellationToken)
    {
        GetCall(callId);
        _sink?.OnCallStateChanged(callId, CallState.Failed, CallEndReason.Rejected, reason.ToString());
        Remove(callId);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task HangupAsync(string callId, CancellationToken cancellationToken)
    {
        GetCall(callId);
        _sink?.OnCallStateChanged(callId, CallState.Disconnecting);
        _sink?.OnCallStateChanged(callId, CallState.Disconnected, CallEndReason.LocalHangup);
        Remove(callId);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SetMutedAsync(string callId, bool muted, CancellationToken cancellationToken)
    {
        GetCall(callId).Muted = muted;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SetHeldAsync(string callId, bool held, CancellationToken cancellationToken)
    {
        GetCall(callId);
        _sink?.OnCallStateChanged(callId, held ? CallState.Holding : CallState.Connecting);
        _sink?.OnCallStateChanged(callId, held ? CallState.Held : CallState.Connected);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SendDtmfAsync(string callId, string digits, CancellationToken cancellationToken)
    {
        GetCall(callId);
        lock (_sync)
        {
            _dtmf.Add(digits);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task TransferAsync(string callId, string destination, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        GetCall(callId);
        _sink?.OnCallStateChanged(callId, CallState.Transferring);
        _sink?.OnCallStateChanged(callId, CallState.Disconnected, CallEndReason.Transferred, destination);
        Remove(callId);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Injects an incoming call. Used by tests and the sample app.
    /// </summary>
    public string SimulateIncoming(string remoteUri, string? displayName = null, bool video = false)
    {
        var uri = SipUri.Normalize(remoteUri);
        var id = NewId();
        lock (_sync)
        {
            _calls[id] = new LoopCall
            {
                Id = id,
                RemoteUri = uri,
                DisplayName = displayName,
                Direction = CallDirection.Incoming
            };
        }

        _sink?.OnIncomingCall(new IncomingCallInfo
        {
            CallId = id,
            RemoteUri = uri,
            DisplayName = displayName,
            Video = video
        });

        return id;
    }

    /// <summary>
    /// Pushes a call into <paramref name="state"/> without going through a public API.
    /// </summary>
    public void ReportCallState(string callId, CallState state, CallEndReason? endReason = null, string? message = null)
    {
        GetCall(callId);
        _sink?.OnCallStateChanged(callId, state, endReason, message);
        if (state is CallState.Disconnected or CallState.Failed)
        {
            Remove(callId);
        }
    }

    async Task ProgressOutgoingAsync(string callId)
    {
        try
        {
            if (_options.DialDelay > TimeSpan.Zero)
            {
                await Task.Delay(_options.DialDelay).ConfigureAwait(false);
            }
            else
            {
                await Task.Yield();
            }

            _sink?.OnCallStateChanged(callId, CallState.Ringing);

            if (_options.AutoAnswerDelay > TimeSpan.Zero)
            {
                await Task.Delay(_options.AutoAnswerDelay).ConfigureAwait(false);
            }

            _sink?.OnCallStateChanged(callId, CallState.Connecting);
            _sink?.OnCallStateChanged(callId, CallState.Connected);
        }
        catch (Exception)
        {
            // The engine may already be disposed.
        }
    }

    LoopCall GetCall(string callId)
    {
        lock (_sync)
        {
            if (_calls.TryGetValue(callId, out var call))
            {
                return call;
            }
        }

        throw new VoipCoreException(VoipCoreError.CallNotFound, $"Loopback has no call '{callId}'.");
    }

    void Remove(string callId)
    {
        lock (_sync)
        {
            _calls.Remove(callId);
        }
    }

    string NewId()
    {
        var n = Interlocked.Increment(ref _sequence);
        return $"loop-{n:D4}";
    }

    sealed class LoopCall
    {
        public required string Id { get; init; }

        public required string RemoteUri { get; init; }

        public string? DisplayName { get; init; }

        public required CallDirection Direction { get; init; }

        public bool Muted { get; set; }
    }
}
