namespace Plugin.Maui.VoipCore;

/// <summary>
/// Pluggable SIP signaling and media engine (PJSIP, Linphone, loopback, or custom).
/// </summary>
public interface ISipStack
{
    /// <summary>
    /// Short name used in diagnostics (for example <c>pjsip</c> or <c>loopback</c>).
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Prepares native libraries and binds the event sink.
    /// </summary>
    Task InitializeAsync(SipStackContext context, CancellationToken cancellationToken);

    /// <summary>
    /// Releases native resources. Active calls should already be hung up.
    /// </summary>
    Task ShutdownAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Sends REGISTER and completes when the registrar accepts or the attempt fails.
    /// </summary>
    Task RegisterAsync(SipAccount account, CancellationToken cancellationToken);

    /// <summary>
    /// Sends unregister and completes when finished.
    /// </summary>
    Task UnregisterAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Starts an outgoing call and returns the stack call identifier.
    /// </summary>
    Task<string> PlaceCallAsync(CallRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Answers an incoming call.
    /// </summary>
    Task AnswerAsync(string callId, CancellationToken cancellationToken);

    /// <summary>
    /// Rejects an incoming call.
    /// </summary>
    Task RejectAsync(string callId, CallRejectReason reason, CancellationToken cancellationToken);

    /// <summary>
    /// Hangs up an active call.
    /// </summary>
    Task HangupAsync(string callId, CancellationToken cancellationToken);

    /// <summary>
    /// Mutes or unmutes the local microphone for a call.
    /// </summary>
    Task SetMutedAsync(string callId, bool muted, CancellationToken cancellationToken);

    /// <summary>
    /// Places a call on hold or resumes it.
    /// </summary>
    Task SetHeldAsync(string callId, bool held, CancellationToken cancellationToken);

    /// <summary>
    /// Sends DTMF digits on the call.
    /// </summary>
    Task SendDtmfAsync(string callId, string digits, CancellationToken cancellationToken);

    /// <summary>
    /// Requests a blind transfer to <paramref name="destination"/>.
    /// </summary>
    Task TransferAsync(string callId, string destination, CancellationToken cancellationToken);
}
