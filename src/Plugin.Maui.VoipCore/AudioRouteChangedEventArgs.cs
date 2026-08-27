namespace Plugin.Maui.VoipCore;

/// <summary>
/// Raised when in-call output routing changes.
/// </summary>
public sealed class AudioRouteChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes event arguments.
    /// </summary>
    public AudioRouteChangedEventArgs(AudioRoute route, bool speakerOn)
    {
        Route = route;
        SpeakerOn = speakerOn;
    }

    /// <summary>
    /// Gets the new route.
    /// </summary>
    public AudioRoute Route { get; }

    /// <summary>
    /// Gets a value indicating whether the loudspeaker is forced on.
    /// </summary>
    public bool SpeakerOn { get; }
}
