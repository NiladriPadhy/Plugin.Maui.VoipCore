namespace Plugin.Maui.VoipCore;

/// <summary>
/// Parameters for an outgoing call.
/// </summary>
public sealed class CallRequest
{
    /// <summary>
    /// Destination as a SIP URI, <c>user@domain</c>, or a phone number.
    /// </summary>
    public required string Destination { get; init; }

    /// <summary>
    /// Optional remote display name for the local UI and CallKit / Telecom.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// When <c>true</c>, the stack should offer video.
    /// </summary>
    public bool Video { get; init; }

    /// <summary>
    /// Optional extra SIP headers.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Headers { get; init; }

    /// <summary>
    /// Returns a copy with a normalized SIP destination.
    /// </summary>
    public CallRequest Normalize()
    {
        var destination = SipUri.Normalize(Destination);
        return ReferenceEquals(destination, Destination) && destination == Destination
            ? this
            : new CallRequest
            {
                Destination = destination,
                DisplayName = DisplayName,
                Video = Video,
                Headers = Headers
            };
    }
}
