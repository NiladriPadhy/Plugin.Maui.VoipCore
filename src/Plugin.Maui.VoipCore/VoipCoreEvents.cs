namespace Plugin.Maui.VoipCore;

/// <summary>
/// Optional diagnostic callbacks configured on <see cref="VoipCoreOptions"/>.
/// </summary>
public sealed class VoipCoreEvents
{
    /// <summary>
    /// Invoked after registrar state changes.
    /// </summary>
    public Action<RegistrationChangedEventArgs>? OnRegistrationChanged { get; set; }

    /// <summary>
    /// Invoked after a call changes.
    /// </summary>
    public Action<CallChangedEventArgs>? OnCallChanged { get; set; }

    /// <summary>
    /// Invoked after an incoming call is created.
    /// </summary>
    public Action<IncomingCallEventArgs>? OnIncomingCall { get; set; }

    /// <summary>
    /// Invoked after the audio route changes.
    /// </summary>
    public Action<AudioRouteChangedEventArgs>? OnAudioRouteChanged { get; set; }
}
