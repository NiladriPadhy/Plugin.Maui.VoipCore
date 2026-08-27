namespace Plugin.Maui.VoipCore;

/// <summary>
/// Lifecycle of the VoIP engine.
/// </summary>
public enum VoipEngineState
{
    /// <summary>
    /// The engine has not been initialized.
    /// </summary>
    Idle = 0,

    /// <summary>
    /// The stack is ready but not registered.
    /// </summary>
    Ready = 1,

    /// <summary>
    /// A REGISTER request is in flight.
    /// </summary>
    Registering = 2,

    /// <summary>
    /// The SIP account is registered.
    /// </summary>
    Registered = 3,

    /// <summary>
    /// An unregister is in flight.
    /// </summary>
    Unregistering = 4,

    /// <summary>
    /// The last register or initialize attempt failed.
    /// </summary>
    Failed = 5
}
