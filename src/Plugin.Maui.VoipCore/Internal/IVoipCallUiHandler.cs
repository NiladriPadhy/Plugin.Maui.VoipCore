namespace Plugin.Maui.VoipCore;

interface IVoipCallUiHandler
{
    Task AnswerFromSystemAsync(string callId);

    Task HangupFromSystemAsync(string callId);

    Task SetMutedFromSystemAsync(string callId, bool muted);
}
