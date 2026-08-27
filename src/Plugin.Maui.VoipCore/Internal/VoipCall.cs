namespace Plugin.Maui.VoipCore;

sealed class VoipCall : IVoipCall
{
    readonly IClock _clock;

    public VoipCall(
        string id,
        CallDirection direction,
        string remoteUri,
        string? displayName,
        bool hasVideo,
        IClock clock)
    {
        Id = id;
        Direction = direction;
        RemoteUri = remoteUri;
        DisplayName = displayName;
        HasVideo = hasVideo;
        _clock = clock;
        StartedAt = clock.UtcNow;
        State = direction == CallDirection.Incoming ? CallState.Ringing : CallState.Dialing;
    }

    public string Id { get; }

    public CallDirection Direction { get; }

    public string RemoteUri { get; }

    public string? DisplayName { get; }

    public CallState State { get; private set; }

    public bool IsMuted { get; private set; }

    public bool IsHeld { get; private set; }

    public bool HasVideo { get; }

    public DateTimeOffset StartedAt { get; }

    public DateTimeOffset? ConnectedAt { get; private set; }

    public DateTimeOffset? EndedAt { get; private set; }

    public TimeSpan Duration
    {
        get
        {
            if (ConnectedAt is not { } connected)
            {
                return TimeSpan.Zero;
            }

            var end = EndedAt ?? _clock.UtcNow;
            var duration = end - connected;
            return duration < TimeSpan.Zero ? TimeSpan.Zero : duration;
        }
    }

    public CallEndReason? EndReason { get; private set; }

    public string? FailureMessage { get; private set; }

    public bool IsTerminal => State is CallState.Disconnected or CallState.Failed;

    public void ApplyState(CallState state, CallEndReason? endReason, string? message, DateTimeOffset now)
    {
        State = state;
        if (state is CallState.Connected && ConnectedAt is null)
        {
            ConnectedAt = now;
        }

        if (state is CallState.Held)
        {
            IsHeld = true;
        }
        else if (state is CallState.Connected or CallState.Connecting)
        {
            IsHeld = false;
        }

        if (state is CallState.Disconnected or CallState.Failed)
        {
            EndedAt ??= now;
            EndReason = endReason ?? (state == CallState.Failed ? CallEndReason.Error : CallEndReason.LocalHangup);
            FailureMessage = message;
        }
        else if (!string.IsNullOrWhiteSpace(message))
        {
            FailureMessage = message;
        }
    }

    public void SetMuted(bool muted) => IsMuted = muted;

    public void SetHeld(bool held)
    {
        IsHeld = held;
        if (held && State == CallState.Connected)
        {
            State = CallState.Held;
        }
        else if (!held && State is CallState.Held or CallState.Holding)
        {
            State = CallState.Connected;
        }
    }
}
