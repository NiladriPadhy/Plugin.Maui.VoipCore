namespace Plugin.Maui.VoipCore;

/// <summary>
/// Output path for in-call audio.
/// </summary>
public enum AudioRoute
{
    /// <summary>
    /// Receiver / earpiece.
    /// </summary>
    Earpiece = 0,

    /// <summary>
    /// Loudspeaker.
    /// </summary>
    Speaker = 1,

    /// <summary>
    /// Bluetooth headset or car kit.
    /// </summary>
    Bluetooth = 2,

    /// <summary>
    /// Wired headset.
    /// </summary>
    Headset = 3,

    /// <summary>
    /// Route is unknown or not yet configured.
    /// </summary>
    Unknown = 4
}
