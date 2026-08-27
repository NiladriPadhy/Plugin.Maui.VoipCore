using Microsoft.Maui.Hosting;

namespace Plugin.Maui.VoipCore;

sealed class VoipCoreInitializer : IMauiInitializeService
{
    public void Initialize(IServiceProvider services)
    {
        var engine = services.GetService<IVoipCore>() ?? VoipCore.Current;
        VoipCore.SetDefault(engine);

        var options = services.GetService<VoipCoreOptions>() ?? engine.Options;
        if (!options.AutoInitialize)
        {
            return;
        }

        _ = InitializeQuietlyAsync(engine);
    }

    static async Task InitializeQuietlyAsync(IVoipCore engine)
    {
        try
        {
            await engine.InitializeAsync().ConfigureAwait(false);
        }
        catch (VoipCoreException)
        {
            // Stay idle/failed. The first explicit operation will surface the error.
        }
    }
}
