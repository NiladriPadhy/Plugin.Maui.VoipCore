namespace Plugin.Maui.VoipCore;

/// <summary>
/// Why a call left the active states.
/// </summary>
public enum CallEndReason
{
    /// <summary>
    /// Local hangup.
    /// </summary>
    LocalHangup = 0,

    /// <summary>
    /// Remote hangup.
    /// </summary>
    RemoteHangup = 1,

    /// <summary>
    /// The call was rejected (busy, decline, or forbidden).
    /// </summary>
    Rejected = 2,

    /// <summary>
    /// The remote party did not answer.
    /// </summary>
    NoAnswer = 3,

    /// <summary>
    /// The call was redirected or transferred.
    /// </summary>
    Transferred = 4,

    /// <summary>
    /// Signaling or media failed.
    /// </summary>
    Error = 5,

    /// <summary>
    /// The engine was shut down.
    /// </summary>
    Shutdown = 6,

    /// <summary>
    /// Another device answered the call.
    /// </summary>
    AnsweredElsewhere = 7
}
