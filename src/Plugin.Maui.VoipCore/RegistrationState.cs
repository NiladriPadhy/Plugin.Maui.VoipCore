namespace Plugin.Maui.VoipCore;

/// <summary>
/// SIP registrar state.
/// </summary>
public enum RegistrationState
{
    /// <summary>
    /// No registration has been attempted.
    /// </summary>
    None = 0,

    /// <summary>
    /// REGISTER is in progress.
    /// </summary>
    Registering = 1,

    /// <summary>
    /// The account is registered.
    /// </summary>
    Registered = 2,

    /// <summary>
    /// Unregister is in progress.
    /// </summary>
    Unregistering = 3,

    /// <summary>
    /// The account is not registered.
    /// </summary>
    Unregistered = 4,

    /// <summary>
    /// Registration failed or was rejected.
    /// </summary>
    Failed = 5
}
