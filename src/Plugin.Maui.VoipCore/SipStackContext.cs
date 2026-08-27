namespace Plugin.Maui.VoipCore;

/// <summary>
/// Runtime context supplied to <see cref="ISipStack.InitializeAsync"/>.
/// </summary>
public sealed class SipStackContext
{
    /// <summary>
    /// Receives registration and call events from the stack.
    /// </summary>
    public required ISipStackSink Sink { get; init; }

    /// <summary>
    /// Engine options.
    /// </summary>
    public required VoipCoreOptions Options { get; init; }
}
