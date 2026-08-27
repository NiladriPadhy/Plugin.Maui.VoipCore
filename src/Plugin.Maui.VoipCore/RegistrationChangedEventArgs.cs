namespace Plugin.Maui.VoipCore;

/// <summary>
/// Registrar state after a transition.
/// </summary>
public sealed class RegistrationChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes event arguments.
    /// </summary>
    public RegistrationChangedEventArgs(RegistrationState state, string? message = null)
    {
        State = state;
        Message = message;
    }

    /// <summary>
    /// Gets the new registrar state.
    /// </summary>
    public RegistrationState State { get; }

    /// <summary>
    /// Gets an optional stack message (for example a SIP reason phrase).
    /// </summary>
    public string? Message { get; }
}
