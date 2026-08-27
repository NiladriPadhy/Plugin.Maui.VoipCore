namespace Plugin.Maui.VoipCore;

/// <summary>
/// SIP credentials and registrar settings.
/// </summary>
public sealed class SipAccount
{
    /// <summary>
    /// SIP user part (for example <c>alice</c>).
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    /// SIP domain / realm (for example <c>sip.example.com</c>).
    /// </summary>
    public required string Domain { get; init; }

    /// <summary>
    /// Optional authentication password.
    /// </summary>
    public string? Password { get; init; }

    /// <summary>
    /// Optional display name placed in the From header.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Authentication username when it differs from <see cref="Username"/>.
    /// </summary>
    public string? AuthUser { get; init; }

    /// <summary>
    /// Registrar host when it differs from <see cref="Domain"/>.
    /// </summary>
    public string? Registrar { get; init; }

    /// <summary>
    /// Outbound proxy.
    /// </summary>
    public string? Proxy { get; init; }

    /// <summary>
    /// Signaling port. Defaults to 5060, or 5061 when <see cref="Transport"/> is TLS.
    /// </summary>
    public int Port { get; init; }

    /// <summary>
    /// Signaling transport.
    /// </summary>
    public SipTransport Transport { get; init; } = SipTransport.Udp;

    /// <summary>
    /// REGISTER expiry. Defaults to <see cref="VoipCoreDefaults.RegistrationExpires"/>.
    /// </summary>
    public TimeSpan Expires { get; init; } = VoipCoreDefaults.RegistrationExpires;

    /// <summary>
    /// Optional STUN server (<c>host:port</c>).
    /// </summary>
    public string? StunServer { get; init; }

    /// <summary>
    /// When <c>true</c>, the stack should enable ICE.
    /// </summary>
    public bool IceEnabled { get; init; }

    /// <summary>
    /// Address of record without a scheme (<c>user@domain</c>).
    /// </summary>
    public string Aor => $"{Username}@{Domain}";

    /// <summary>
    /// SIP URI for this account.
    /// </summary>
    public string SipUri => Transport == SipTransport.Tls ? $"sips:{Aor}" : $"sip:{Aor}";

    /// <summary>
    /// Effective registrar host.
    /// </summary>
    public string RegistrarHost => string.IsNullOrWhiteSpace(Registrar) ? Domain : Registrar;

    /// <summary>
    /// Effective signaling port.
    /// </summary>
    public int EffectivePort =>
        Port > 0 ? Port : Transport == SipTransport.Tls ? VoipCoreDefaults.SipsPort : VoipCoreDefaults.SipPort;

    /// <summary>
    /// Throws <see cref="VoipCoreException"/> when the account cannot be used.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            throw new VoipCoreException(VoipCoreError.InvalidAccount, "SipAccount.Username is required.");
        }

        if (string.IsNullOrWhiteSpace(Domain))
        {
            throw new VoipCoreException(VoipCoreError.InvalidAccount, "SipAccount.Domain is required.");
        }

        if (Port is < 0 or > 65535)
        {
            throw new VoipCoreException(VoipCoreError.InvalidAccount, "SipAccount.Port must be between 0 and 65535.");
        }

        if (Expires <= TimeSpan.Zero)
        {
            throw new VoipCoreException(VoipCoreError.InvalidAccount, "SipAccount.Expires must be greater than zero.");
        }
    }
}
