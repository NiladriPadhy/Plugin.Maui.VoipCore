namespace Plugin.Maui.VoipCore;

/// <summary>
/// A live SIP call session owned by <see cref="IVoipCore"/>.
/// </summary>
public interface IVoipCall
{
    /// <summary>
    /// Stack-assigned identifier.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Incoming or outgoing.
    /// </summary>
    CallDirection Direction { get; }

    /// <summary>
    /// Remote SIP URI.
    /// </summary>
    string RemoteUri { get; }

    /// <summary>
    /// Remote display name when known.
    /// </summary>
    string? DisplayName { get; }

    /// <summary>
    /// Current signaling state.
    /// </summary>
    CallState State { get; }

    /// <summary>
    /// When <c>true</c>, the local microphone is muted.
    /// </summary>
    bool IsMuted { get; }

    /// <summary>
    /// When <c>true</c>, the call is on hold.
    /// </summary>
    bool IsHeld { get; }

    /// <summary>
    /// When <c>true</c>, the invite offered or requested video.
    /// </summary>
    bool HasVideo { get; }

    /// <summary>
    /// UTC time the local session was created.
    /// </summary>
    DateTimeOffset StartedAt { get; }

    /// <summary>
    /// UTC time media became connected, if ever.
    /// </summary>
    DateTimeOffset? ConnectedAt { get; }

    /// <summary>
    /// UTC time the call reached a terminal state.
    /// </summary>
    DateTimeOffset? EndedAt { get; }

    /// <summary>
    /// Connected duration, or <see cref="TimeSpan.Zero"/> when the call never connected.
    /// </summary>
    TimeSpan Duration { get; }

    /// <summary>
    /// Why the call ended, when it has ended.
    /// </summary>
    CallEndReason? EndReason { get; }

    /// <summary>
    /// Stack or engine message for a failure.
    /// </summary>
    string? FailureMessage { get; }

    /// <summary>
    /// When <c>true</c>, the call will not become active again.
    /// </summary>
    bool IsTerminal { get; }
}
