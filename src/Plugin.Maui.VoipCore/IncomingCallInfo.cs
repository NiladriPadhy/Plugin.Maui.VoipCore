namespace Plugin.Maui.VoipCore;

/// <summary>
/// Payload raised by a SIP stack when an INVITE arrives.
/// </summary>
public sealed class IncomingCallInfo
{
    /// <summary>
    /// Stack-assigned call identifier.
    /// </summary>
    public required string CallId { get; init; }

    /// <summary>
    /// Remote SIP URI.
    /// </summary>
    public required string RemoteUri { get; init; }

    /// <summary>
    /// Optional caller display name.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// When <c>true</c>, the invite offered video.
    /// </summary>
    public bool Video { get; init; }
}
