#if IOS
using CallKit;
using CoreFoundation;
using Foundation;

namespace Plugin.Maui.VoipCore;

sealed class PlatformCallUi : IVoipCallUi
{
    readonly VoipCoreOptions _options;
    readonly Dictionary<string, NSUuid> _ids = new(StringComparer.Ordinal);
    readonly Dictionary<string, string> _idsByUuid = new(StringComparer.OrdinalIgnoreCase);
    readonly object _sync = new();
    CXProvider? _provider;
    IVoipCallUiHandler? _handler;

    public PlatformCallUi(VoipCoreOptions options)
    {
        _options = options;
    }

    public void Attach(IVoipCallUiHandler handler) => _handler = handler;

    public Task ReportOutgoingAsync(IVoipCall call, CancellationToken cancellationToken)
    {
        if (!TryGetProvider(out var provider))
        {
            return Task.CompletedTask;
        }

        var uuid = Bind(call.Id);
        provider.ReportConnectingOutgoingCall(uuid, (NSDate)DateTime.UtcNow);
        return Task.CompletedTask;
    }

    public Task ReportIncomingAsync(IVoipCall call, CancellationToken cancellationToken)
    {
        if (!TryGetProvider(out var provider))
        {
            return Task.CompletedTask;
        }

        var uuid = Bind(call.Id);
        var update = new CXCallUpdate
        {
            RemoteHandle = new CXHandle(CXHandleType.Generic, call.RemoteUri),
            LocalizedCallerName = call.DisplayName ?? call.RemoteUri,
            HasVideo = call.HasVideo,
            SupportsDtmf = true,
            SupportsHolding = true,
            SupportsGrouping = false,
            SupportsUngrouping = false
        };

        var tcs = new TaskCompletionSource();
        provider.ReportNewIncomingCall(uuid, update, error =>
        {
            if (error is not null)
            {
                tcs.TrySetException(new VoipCoreException(
                    VoipCoreError.CallFailed,
                    $"CallKit rejected the incoming call: {error.LocalizedDescription}"));
                return;
            }

            tcs.TrySetResult();
        });

        return tcs.Task.WaitAsync(cancellationToken);
    }

    public Task ReportConnectedAsync(IVoipCall call, CancellationToken cancellationToken)
    {
        if (!TryGetProvider(out var provider) || !TryGetUuid(call.Id, out var uuid))
        {
            return Task.CompletedTask;
        }

        provider.ReportConnectedOutgoingCall(uuid, (NSDate)DateTime.UtcNow);
        return Task.CompletedTask;
    }

    public Task ReportEndedAsync(IVoipCall call, CancellationToken cancellationToken)
    {
        if (!TryGetProvider(out var provider) || !TryGetUuid(call.Id, out var uuid))
        {
            return Task.CompletedTask;
        }

        var reason = call.EndReason switch
        {
            CallEndReason.RemoteHangup => CXCallEndedReason.RemoteEnded,
            CallEndReason.Rejected => CXCallEndedReason.DeclinedElsewhere,
            CallEndReason.NoAnswer => CXCallEndedReason.Unanswered,
            CallEndReason.AnsweredElsewhere => CXCallEndedReason.AnsweredElsewhere,
            CallEndReason.Error => CXCallEndedReason.Failed,
            _ => CXCallEndedReason.RemoteEnded
        };

        provider.ReportCall(uuid, (NSDate)DateTime.UtcNow, reason);
        Unbind(call.Id, uuid);
        return Task.CompletedTask;
    }

    bool TryGetProvider(out CXProvider provider)
    {
        provider = null!;
        if (!_options.UseNativeCallUi)
        {
            return false;
        }

        if (_provider is not null)
        {
            provider = _provider;
            return true;
        }

        var configuration = new CXProviderConfiguration
        {
            MaximumCallGroups = 1,
            MaximumCallsPerCallGroup = (nuint)Math.Max(1, _options.MaxConcurrentCalls),
            SupportsVideo = false
        };

        _provider = new CXProvider(configuration);
        _provider.SetDelegate(new ProviderDelegate(this), DispatchQueue.MainQueue);
        provider = _provider;
        return true;
    }

    NSUuid Bind(string callId)
    {
        lock (_sync)
        {
            if (_ids.TryGetValue(callId, out var existing))
            {
                return existing;
            }

            var uuid = new NSUuid();
            _ids[callId] = uuid;
            _idsByUuid[uuid.ToString()] = callId;
            return uuid;
        }
    }

    bool TryGetUuid(string callId, out NSUuid uuid)
    {
        lock (_sync)
        {
            return _ids.TryGetValue(callId, out uuid!);
        }
    }

    void Unbind(string callId, NSUuid uuid)
    {
        lock (_sync)
        {
            _ids.Remove(callId);
            _idsByUuid.Remove(uuid.ToString());
        }
    }

    bool TryResolve(NSUuid uuid, out string callId)
    {
        lock (_sync)
        {
            return _idsByUuid.TryGetValue(uuid.ToString(), out callId!);
        }
    }

    sealed class ProviderDelegate : CXProviderDelegate
    {
        readonly PlatformCallUi _owner;

        public ProviderDelegate(PlatformCallUi owner) => _owner = owner;

        public override void DidReset(CXProvider provider)
        {
        }

        public override void PerformAnswerCallAction(CXProvider provider, CXAnswerCallAction action)
        {
            if (_owner.TryResolve(action.CallUuid, out var callId))
            {
                _ = _owner._handler?.AnswerFromSystemAsync(callId);
            }

            action.Fulfill();
        }

        public override void PerformEndCallAction(CXProvider provider, CXEndCallAction action)
        {
            if (_owner.TryResolve(action.CallUuid, out var callId))
            {
                _ = _owner._handler?.HangupFromSystemAsync(callId);
            }

            action.Fulfill();
        }

        public override void PerformSetMutedCallAction(CXProvider provider, CXSetMutedCallAction action)
        {
            if (_owner.TryResolve(action.CallUuid, out var callId))
            {
                _ = _owner._handler?.SetMutedFromSystemAsync(callId, action.Muted);
            }

            action.Fulfill();
        }
    }
}
#endif
