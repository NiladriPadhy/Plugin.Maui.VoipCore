namespace Plugin.Maui.VoipCore;

/// <summary>
/// Generic SIP/VoIP facade for .NET MAUI.
/// </summary>
public interface IVoipCore
{
    /// <summary>
    /// Gets a value indicating whether this target can run the engine.
    /// Always <c>true</c> for Android, iOS, and the shared <c>net10.0</c> surface.
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// Gets a value indicating whether <see cref="InitializeAsync"/> has completed.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Gets the engine lifecycle state.
    /// </summary>
    VoipEngineState State { get; }

    /// <summary>
    /// Gets the SIP registrar state.
    /// </summary>
    RegistrationState Registration { get; }

    /// <summary>
    /// Gets the account used for the last successful or in-flight register.
    /// </summary>
    SipAccount? Account { get; }

    /// <summary>
    /// Gets the active SIP stack.
    /// </summary>
    ISipStack Stack { get; }

    /// <summary>
    /// Gets in-call audio routing.
    /// </summary>
    IVoipAudio Audio { get; }

    /// <summary>
    /// Gets the options this instance was created with.
    /// </summary>
    VoipCoreOptions Options { get; }

    /// <summary>
    /// Gets every known call, including terminal ones still retained.
    /// </summary>
    IReadOnlyList<IVoipCall> Calls { get; }

    /// <summary>
    /// Gets the preferred in-progress call, or <c>null</c>.
    /// </summary>
    IVoipCall? ActiveCall { get; }

    /// <summary>
    /// Raised when registrar state changes.
    /// </summary>
    event EventHandler<RegistrationChangedEventArgs>? RegistrationChanged;

    /// <summary>
    /// Raised when any call changes state or media flags.
    /// </summary>
    event EventHandler<CallChangedEventArgs>? CallChanged;

    /// <summary>
    /// Raised when a new incoming call arrives.
    /// </summary>
    event EventHandler<IncomingCallEventArgs>? IncomingCall;

    /// <summary>
    /// Raised when the output audio route changes.
    /// </summary>
    event EventHandler<AudioRouteChangedEventArgs>? AudioRouteChanged;

    /// <summary>
    /// Prepares the SIP stack and platform audio. Safe to call more than once.
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Hangs up calls, unregisters, and releases the stack.
    /// </summary>
    Task ShutdownAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers <paramref name="account"/> with the SIP registrar.
    /// </summary>
    Task RegisterAsync(SipAccount account, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unregisters the current account.
    /// </summary>
    Task UnregisterAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Places an outgoing call.
    /// </summary>
    Task<IVoipCall> PlaceCallAsync(CallRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Answers an incoming call.
    /// </summary>
    Task<IVoipCall> AnswerAsync(string callId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rejects an incoming call.
    /// </summary>
    Task RejectAsync(string callId, CallRejectReason reason = CallRejectReason.Decline, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hangs up a call.
    /// </summary>
    Task HangupAsync(string callId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hangs up every non-terminal call.
    /// </summary>
    Task HangupAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Mutes or unmutes the local microphone.
    /// </summary>
    Task SetMutedAsync(string callId, bool muted, CancellationToken cancellationToken = default);

    /// <summary>
    /// Places a call on hold or resumes it.
    /// </summary>
    Task SetHeldAsync(string callId, bool held, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends RFC 4733 / SIP INFO DTMF digits.
    /// </summary>
    Task SendDtmfAsync(string callId, string digits, CancellationToken cancellationToken = default);

    /// <summary>
    /// Blind-transfers a connected call.
    /// </summary>
    Task TransferAsync(string callId, string destination, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a live call, or <c>null</c> when it is unknown.
    /// </summary>
    IVoipCall? GetCall(string callId);

    /// <summary>
    /// Requests the microphone permission used for calls.
    /// </summary>
    Task<bool> EnsureMicrophoneAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Called when the app returns to the foreground.
    /// </summary>
    void NotifyForeground();

    /// <summary>
    /// Called when the app moves to the background. Holds connected calls when configured.
    /// </summary>
    void NotifyBackground();
}
