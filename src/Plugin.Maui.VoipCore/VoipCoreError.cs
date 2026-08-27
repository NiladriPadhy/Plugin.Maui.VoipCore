namespace Plugin.Maui.VoipCore;

/// <summary>
/// Classifies a <see cref="VoipCoreException"/>.
/// </summary>
public enum VoipCoreError
{
    /// <summary>
    /// The operation is not valid in the current engine or call state.
    /// </summary>
    InvalidOperation = 0,

    /// <summary>
    /// <see cref="IVoipCore.InitializeAsync"/> has not completed.
    /// </summary>
    NotInitialized = 1,

    /// <summary>
    /// The SIP account is not registered.
    /// </summary>
    NotRegistered = 2,

    /// <summary>
    /// The SIP account is missing required fields or has invalid values.
    /// </summary>
    InvalidAccount = 3,

    /// <summary>
    /// The call destination is empty or cannot be normalized.
    /// </summary>
    InvalidDestination = 4,

    /// <summary>
    /// The DTMF string contains characters outside <c>0-9</c>, <c>*</c>, <c>#</c>, and <c>A-D</c>.
    /// </summary>
    InvalidDtmf = 5,

    /// <summary>
    /// No call exists for the supplied identifier.
    /// </summary>
    CallNotFound = 6,

    /// <summary>
    /// <see cref="VoipCoreOptions.MaxConcurrentCalls"/> has been reached.
    /// </summary>
    CallLimitReached = 7,

    /// <summary>
    /// SIP registration failed.
    /// </summary>
    RegistrationFailed = 8,

    /// <summary>
    /// The SIP stack could not set up or continue the call.
    /// </summary>
    CallFailed = 9,

    /// <summary>
    /// Platform audio could not be configured.
    /// </summary>
    AudioFailed = 10,

    /// <summary>
    /// The pluggable <see cref="ISipStack"/> reported a failure.
    /// </summary>
    StackFailure = 11,

    /// <summary>
    /// The engine instance has been disposed.
    /// </summary>
    Disposed = 12
}
