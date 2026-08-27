#if !ANDROID && !IOS
namespace Plugin.Maui.VoipCore;

sealed class PlatformCallUi : IVoipCallUi
{
    public PlatformCallUi(VoipCoreOptions options)
    {
        _ = options;
    }

    public void Attach(IVoipCallUiHandler handler)
    {
        _ = handler;
    }

    public Task ReportOutgoingAsync(IVoipCall call, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task ReportIncomingAsync(IVoipCall call, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task ReportConnectedAsync(IVoipCall call, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task ReportEndedAsync(IVoipCall call, CancellationToken cancellationToken) => Task.CompletedTask;
}
#endif
