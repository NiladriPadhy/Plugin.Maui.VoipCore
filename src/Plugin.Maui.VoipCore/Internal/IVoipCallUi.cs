namespace Plugin.Maui.VoipCore;

interface IVoipCallUi
{
    void Attach(IVoipCallUiHandler handler);

    Task ReportOutgoingAsync(IVoipCall call, CancellationToken cancellationToken);

    Task ReportIncomingAsync(IVoipCall call, CancellationToken cancellationToken);

    Task ReportConnectedAsync(IVoipCall call, CancellationToken cancellationToken);

    Task ReportEndedAsync(IVoipCall call, CancellationToken cancellationToken);
}
