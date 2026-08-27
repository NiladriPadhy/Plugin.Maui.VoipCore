namespace Plugin.Maui.VoipCore;

sealed class VoipAudio : IVoipAudio
{
    readonly IVoipAudioSession _session;
    readonly Action<AudioRoute, bool> _changed;

    public VoipAudio(IVoipAudioSession session, Action<AudioRoute, bool> changed)
    {
        _session = session;
        _changed = changed;
    }

    public bool IsSpeakerOn => _session.IsSpeakerOn;

    public AudioRoute Route => _session.Route;

    public async Task SetSpeakerAsync(bool enabled, CancellationToken cancellationToken = default)
    {
        await _session.SetSpeakerAsync(enabled, cancellationToken).ConfigureAwait(false);
        _changed(_session.Route, _session.IsSpeakerOn);
    }
}
