#if IOS
using AVFoundation;

namespace Plugin.Maui.VoipCore;

sealed class PlatformAudioSession : IVoipAudioSession
{
    public bool IsSpeakerOn { get; private set; }

    public AudioRoute Route => IsSpeakerOn ? AudioRoute.Speaker : AudioRoute.Earpiece;

    public Task ConfigureForCallAsync(CancellationToken cancellationToken)
    {
        var session = AVAudioSession.SharedInstance();
        session.SetCategory(
            AVAudioSessionCategory.PlayAndRecord,
            AVAudioSessionCategoryOptions.AllowBluetooth
            | AVAudioSessionCategoryOptions.AllowBluetoothA2DP
            | AVAudioSessionCategoryOptions.DefaultToSpeaker,
            out var categoryError);
        ThrowIfAudioFailed(categoryError, "AVAudioSession category");

        session.SetMode(AVAudioSessionMode.VoiceChat, out var modeError);
        ThrowIfAudioFailed(modeError, "AVAudioSession mode");

        session.SetActive(true, out var activeError);
        ThrowIfAudioFailed(activeError, "AVAudioSession activation");
        return Task.CompletedTask;
    }

    public Task SetSpeakerAsync(bool enabled, CancellationToken cancellationToken)
    {
        IsSpeakerOn = enabled;
        var session = AVAudioSession.SharedInstance();
        session.OverrideOutputAudioPort(
            enabled ? AVAudioSessionPortOverride.Speaker : AVAudioSessionPortOverride.None,
            out var error);
        ThrowIfAudioFailed(error, "AVAudioSession speaker override");
        return Task.CompletedTask;
    }

    public Task RestoreAsync(CancellationToken cancellationToken)
    {
        var session = AVAudioSession.SharedInstance();
        session.OverrideOutputAudioPort(AVAudioSessionPortOverride.None, out _);
        session.SetActive(false, AVAudioSessionSetActiveOptions.NotifyOthersOnDeactivation, out _);
        IsSpeakerOn = false;
        return Task.CompletedTask;
    }

    static void ThrowIfAudioFailed(Foundation.NSError? error, string operation)
    {
        if (error is not null)
        {
            throw new VoipCoreException(
                VoipCoreError.AudioFailed,
                $"{operation} failed: {error.LocalizedDescription}");
        }
    }
}
#endif
