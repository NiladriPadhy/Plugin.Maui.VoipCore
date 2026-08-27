namespace Plugin.Maui.VoipCore;

/// <summary>
/// Default values used when <see cref="VoipCoreOptions"/> does not override them.
/// </summary>
public static class VoipCoreDefaults
{
    /// <summary>
    /// Default SIP signaling port.
    /// </summary>
    public const int SipPort = 5060;

    /// <summary>
    /// Default SIPS port.
    /// </summary>
    public const int SipsPort = 5061;

    /// <summary>
    /// Maximum simultaneous non-terminal calls.
    /// </summary>
    public const int MaxConcurrentCalls = 2;

    /// <summary>
    /// Default REGISTER expiry.
    /// </summary>
    public static readonly TimeSpan RegistrationExpires = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Default time to wait for a REGISTER response.
    /// </summary>
    public static readonly TimeSpan RegistrationTimeout = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Default time to wait for a call to connect.
    /// </summary>
    public static readonly TimeSpan CallSetupTimeout = TimeSpan.FromSeconds(60);
}
