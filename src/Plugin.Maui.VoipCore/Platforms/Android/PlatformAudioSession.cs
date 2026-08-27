#if ANDROID
using Android.Media;
using AndroidApp = Android.App.Application;
using AndroidContext = Android.Content.Context;

namespace Plugin.Maui.VoipCore;

sealed class PlatformAudioSession : IVoipAudioSession
{
    AudioManager? _manager;
    Mode _previousMode;
    bool _previousSpeaker;
    bool _configured;

    public bool IsSpeakerOn { get; private set; }

    public AudioRoute Route => IsSpeakerOn ? AudioRoute.Speaker : AudioRoute.Earpiece;

    public Task ConfigureForCallAsync(CancellationToken cancellationToken)
    {
        var manager = Manager();
        _previousMode = manager.Mode;
        _previousSpeaker = manager.SpeakerphoneOn;
        manager.Mode = Mode.InCommunication;
        manager.SpeakerphoneOn = IsSpeakerOn;
        _configured = true;
        return Task.CompletedTask;
    }

    public Task SetSpeakerAsync(bool enabled, CancellationToken cancellationToken)
    {
        IsSpeakerOn = enabled;
        if (_configured)
        {
            var manager = Manager();
            manager.Mode = Mode.InCommunication;
            manager.SpeakerphoneOn = enabled;
        }

        return Task.CompletedTask;
    }

    public Task RestoreAsync(CancellationToken cancellationToken)
    {
        if (!_configured)
        {
            IsSpeakerOn = false;
            return Task.CompletedTask;
        }

        try
        {
            var manager = Manager();
            manager.SpeakerphoneOn = _previousSpeaker;
            manager.Mode = _previousMode;
        }
        catch (Exception)
        {
            // Restoring AudioManager is best-effort.
        }

        _configured = false;
        IsSpeakerOn = false;
        return Task.CompletedTask;
    }

    AudioManager Manager()
    {
        if (_manager is not null)
        {
            return _manager;
        }

        var context = AndroidApp.Context
            ?? throw new VoipCoreException(VoipCoreError.AudioFailed, "The Android application context is not available.");

        _manager = context.GetSystemService(AndroidContext.AudioService) as AudioManager
            ?? throw new VoipCoreException(VoipCoreError.AudioFailed, "Android AudioManager is not available.");

        return _manager;
    }
}
#endif
