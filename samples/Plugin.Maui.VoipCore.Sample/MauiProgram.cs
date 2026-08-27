using Microsoft.Extensions.Logging;
using Plugin.Maui.VoipCore;

namespace Plugin.Maui.VoipCore.Sample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.Services.AddSingleton<MainPage>();

        builder
            .UseMauiApp<App>()
            .UseVoipCore(options =>
            {
                options.Account = new SipAccount
                {
                    Username = "alice",
                    Domain = "sip.example.com",
                    DisplayName = "Alice"
                };
                options.AutoRegister = true;
                options.HoldOnBackground = true;
                options.UseNativeCallUi = true;
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
