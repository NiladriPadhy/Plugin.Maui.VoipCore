namespace Plugin.Maui.VoipCore;

/// <summary>
/// Reason sent when rejecting an incoming call.
/// </summary>
public enum CallRejectReason
{
    /// <summary>
    /// Decline the invite (603).
    /// </summary>
    Decline = 0,

    /// <summary>
    /// Busy here (486).
    /// </summary>
    Busy = 1,

    /// <summary>
    /// Temporarily unavailable (480).
    /// </summary>
    TemporarilyUnavailable = 2
}
