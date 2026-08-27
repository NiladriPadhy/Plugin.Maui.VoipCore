namespace Plugin.Maui.VoipCore;

/// <summary>
/// Behavior of <see cref="LoopbackSipStack"/>.
/// </summary>
public sealed class LoopbackSipStackOptions
{
    /// <summary>
    /// When <c>true</c>, outgoing calls progress to <see cref="CallState.Connected"/> after
    /// <see cref="ISipStack.PlaceCallAsync"/> returns. Tests usually set this to <c>false</c>.
    /// </summary>
    public bool AutoProgress { get; set; } = true;

    /// <summary>
    /// Delay before an auto-progressed outgoing call reports ringing.
    /// </summary>
    public TimeSpan DialDelay { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Delay after ringing before auto-answer.
    /// </summary>
    public TimeSpan AutoAnswerDelay { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// When <c>true</c>, <see cref="ISipStack.RegisterAsync"/> reports failure.
    /// </summary>
    public bool FailRegistration { get; set; }

    /// <summary>
    /// Optional message stored when <see cref="FailRegistration"/> is <c>true</c>.
    /// </summary>
    public string? FailRegistrationMessage { get; set; }

    /// <summary>
    /// When <c>true</c>, <see cref="ISipStack.PlaceCallAsync"/> fails.
    /// </summary>
    public bool FailCalls { get; set; }
}
