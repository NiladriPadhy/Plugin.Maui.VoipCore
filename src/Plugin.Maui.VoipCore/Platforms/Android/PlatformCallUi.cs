#if ANDROID
namespace Plugin.Maui.VoipCore;

/// <summary>
/// Android in-app call reporting. Telecom <c>ConnectionService</c> must be hosted by the
/// consuming app; VoipCore surfaces incoming calls through <see cref="IVoipCore.IncomingCall"/>.
/// </summary>
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
