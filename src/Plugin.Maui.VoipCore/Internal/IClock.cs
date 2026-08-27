namespace Plugin.Maui.VoipCore;

internal interface IClock
{
    DateTimeOffset UtcNow { get; }
}
