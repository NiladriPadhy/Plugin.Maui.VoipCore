namespace Plugin.Maui.VoipCore;

interface IVoipAudioSession
{
    bool IsSpeakerOn { get; }

    AudioRoute Route { get; }

    Task ConfigureForCallAsync(CancellationToken cancellationToken);

    Task SetSpeakerAsync(bool enabled, CancellationToken cancellationToken);

    Task RestoreAsync(CancellationToken cancellationToken);
}
