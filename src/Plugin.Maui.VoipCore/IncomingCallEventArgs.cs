namespace Plugin.Maui.VoipCore;

/// <summary>
/// Raised when a new incoming call is available.
/// </summary>
public sealed class IncomingCallEventArgs : EventArgs
{
    /// <summary>
    /// Initializes event arguments.
    /// </summary>
    public IncomingCallEventArgs(IVoipCall call)
    {
        Call = call;
    }

    /// <summary>
    /// Gets the ringing incoming call.
    /// </summary>
    public IVoipCall Call { get; }
}
