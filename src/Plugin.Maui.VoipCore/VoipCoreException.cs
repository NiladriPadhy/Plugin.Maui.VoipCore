namespace Plugin.Maui.VoipCore;

/// <summary>
/// Thrown when a VoIP operation cannot be completed.
/// </summary>
public sealed class VoipCoreException : Exception
{
    /// <summary>
    /// Initializes a new exception with an error code and message.
    /// </summary>
    public VoipCoreException(VoipCoreError error, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Error = error;
    }

    /// <summary>
    /// Gets the classified error.
    /// </summary>
    public VoipCoreError Error { get; }
}
