namespace Plugin.Maui.VoipCore;

/// <summary>
/// Configuration for a <see cref="IVoipCore"/> instance.
/// </summary>
public sealed class VoipCoreOptions
{
    /// <summary>
    /// Account used when <see cref="AutoRegister"/> is <c>true</c>.
    /// </summary>
    public SipAccount? Account { get; set; }

    /// <summary>
    /// When <c>true</c>, <see cref="IVoipCore.InitializeAsync"/> also registers <see cref="Account"/>.
    /// </summary>
    public bool AutoRegister { get; set; }

    /// <summary>
    /// When <c>true</c>, <c>UseVoipCore</c> starts the engine during MAUI initialization.
    /// </summary>
    public bool AutoInitialize { get; set; } = true;

    /// <summary>
    /// When <c>true</c>, outgoing calls start on the loudspeaker.
    /// </summary>
    public bool UseSpeakerByDefault { get; set; }

    /// <summary>
    /// When <c>true</c>, iOS reports calls through CallKit. Android still raises
    /// <see cref="IVoipCore.IncomingCall"/> for in-app UI (Telecom requires an app-level service).
    /// </summary>
    public bool UseNativeCallUi { get; set; } = true;

    /// <summary>
    /// When <c>true</c>, connected calls are placed on hold when the app backgrounds.
    /// </summary>
    public bool HoldOnBackground { get; set; } = true;

    /// <summary>
    /// When <c>true</c>, the account is unregistered when the app backgrounds.
    /// </summary>
    public bool UnregisterOnBackground { get; set; }

    /// <summary>
    /// Maximum non-terminal calls. Defaults to <see cref="VoipCoreDefaults.MaxConcurrentCalls"/>.
    /// </summary>
    public int MaxConcurrentCalls { get; set; } = VoipCoreDefaults.MaxConcurrentCalls;

    /// <summary>
    /// Time the engine waits for REGISTER to complete.
    /// </summary>
    public TimeSpan RegistrationTimeout { get; set; } = VoipCoreDefaults.RegistrationTimeout;

    /// <summary>
    /// Advisory timeout for call setup. Stacks may honor this.
    /// </summary>
    public TimeSpan CallSetupTimeout { get; set; } = VoipCoreDefaults.CallSetupTimeout;

    /// <summary>
    /// Factory for the SIP stack. When <c>null</c>, <see cref="LoopbackSipStack"/> is used.
    /// </summary>
    public Func<ISipStack>? StackFactory { get; set; }

    /// <summary>
    /// Diagnostic callbacks.
    /// </summary>
    public VoipCoreEvents Events { get; set; } = new();
}
