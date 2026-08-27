#if !ANDROID && !IOS
namespace Plugin.Maui.VoipCore;

sealed class PlatformAudioSession : IVoipAudioSession
{
    public bool IsSpeakerOn { get; private set; }

    public AudioRoute Route => IsSpeakerOn ? AudioRoute.Speaker : AudioRoute.Earpiece;

    public Task ConfigureForCallAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task SetSpeakerAsync(bool enabled, CancellationToken cancellationToken)
    {
        IsSpeakerOn = enabled;
        return Task.CompletedTask;
    }

    public Task RestoreAsync(CancellationToken cancellationToken)
    {
        IsSpeakerOn = false;
        return Task.CompletedTask;
    }
}
#endif
