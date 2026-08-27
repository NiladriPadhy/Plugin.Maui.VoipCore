namespace Plugin.Maui.VoipCore;

/// <summary>
/// SIP signaling transport.
/// </summary>
public enum SipTransport
{
    /// <summary>
    /// UDP (default SIP).
    /// </summary>
    Udp = 0,

    /// <summary>
    /// TCP.
    /// </summary>
    Tcp = 1,

    /// <summary>
    /// TLS (SIPS).
    /// </summary>
    Tls = 2
}
