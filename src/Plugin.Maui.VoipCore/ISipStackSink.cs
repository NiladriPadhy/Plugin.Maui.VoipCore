namespace Plugin.Maui.VoipCore;

/// <summary>
/// Callbacks a SIP stack uses to notify <see cref="IVoipCore"/> of signaling changes.
/// </summary>
public interface ISipStackSink
{
    /// <summary>
    /// Reports a registrar state change.
    /// </summary>
    void OnRegistrationChanged(RegistrationState state, string? message = null);

    /// <summary>
    /// Reports a new incoming INVITE.
    /// </summary>
    void OnIncomingCall(IncomingCallInfo info);

    /// <summary>
    /// Reports a call state transition.
    /// </summary>
    void OnCallStateChanged(string callId, CallState state, CallEndReason? endReason = null, string? message = null);
}
