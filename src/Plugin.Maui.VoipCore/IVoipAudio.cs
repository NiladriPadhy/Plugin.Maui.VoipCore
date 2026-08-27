namespace Plugin.Maui.VoipCore;

/// <summary>
/// In-call audio routing.
/// </summary>
public interface IVoipAudio
{
    /// <summary>
    /// When <c>true</c>, output is forced to the loudspeaker.
    /// </summary>
    bool IsSpeakerOn { get; }

    /// <summary>
    /// Current output route.
    /// </summary>
    AudioRoute Route { get; }

    /// <summary>
    /// Enables or disables the loudspeaker.
    /// </summary>
    Task SetSpeakerAsync(bool enabled, CancellationToken cancellationToken = default);
}
