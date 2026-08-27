namespace Plugin.Maui.VoipCore;

/// <summary>
/// Direction of a SIP call relative to this endpoint.
/// </summary>
public enum CallDirection
{
    /// <summary>
    /// This endpoint placed the call.
    /// </summary>
    Outgoing = 0,

    /// <summary>
    /// This endpoint received the call.
    /// </summary>
    Incoming = 1
}
