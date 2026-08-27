namespace Plugin.Maui.VoipCore;

/// <summary>
/// Session state of a SIP call.
/// </summary>
public enum CallState
{
    /// <summary>
    /// The call object exists but no signaling has started.
    /// </summary>
    Idle = 0,

    /// <summary>
    /// An outgoing INVITE has been sent.
    /// </summary>
    Dialing = 1,

    /// <summary>
    /// The remote party is ringing, or an incoming INVITE is waiting.
    /// </summary>
    Ringing = 2,

    /// <summary>
    /// Early media is being exchanged.
    /// </summary>
    EarlyMedia = 3,

    /// <summary>
    /// The call is being established (200 OK / ACK in flight).
    /// </summary>
    Connecting = 4,

    /// <summary>
    /// The call is connected and media should flow.
    /// </summary>
    Connected = 5,

    /// <summary>
    /// A hold request is in flight.
    /// </summary>
    Holding = 6,

    /// <summary>
    /// The call is on hold.
    /// </summary>
    Held = 7,

    /// <summary>
    /// A transfer is in flight.
    /// </summary>
    Transferring = 8,

    /// <summary>
    /// Hangup signaling is in flight.
    /// </summary>
    Disconnecting = 9,

    /// <summary>
    /// The call has ended normally.
    /// </summary>
    Disconnected = 10,

    /// <summary>
    /// The call failed or was rejected.
    /// </summary>
    Failed = 11
}
