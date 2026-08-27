namespace Plugin.Maui.VoipCore;

/// <summary>
/// Raised after a call changes state or media flags.
/// </summary>
public sealed class CallChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes event arguments.
    /// </summary>
    public CallChangedEventArgs(IVoipCall call)
    {
        Call = call;
    }

    /// <summary>
    /// Gets the call that changed.
    /// </summary>
    public IVoipCall Call { get; }
}
