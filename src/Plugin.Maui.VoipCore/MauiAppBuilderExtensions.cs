using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Plugin.Maui.VoipCore;

/// <summary>
/// MAUI host registration for VoipCore.
/// </summary>
public static class MauiAppBuilderExtensions
{
    /// <summary>
    /// Registers <see cref="IVoipCore"/> as a singleton and wires Android/iOS lifecycle hooks
    /// for hold-on-background.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.UseVoipCore(options =>
    /// {
    ///     options.Account = new SipAccount { Username = "alice", Domain = "sip.example.com" };
    ///     options.AutoRegister = true;
    /// });
    /// </code>
    /// </example>
    public static MauiAppBuilder UseVoipCore(this MauiAppBuilder builder, Action<VoipCoreOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var options = new VoipCoreOptions();
        configure?.Invoke(options);

        builder.Services.AddVoipCore(options);
        builder.Services.AddTransient<IMauiInitializeService, VoipCoreInitializer>();

        builder.ConfigureLifecycleEvents(events =>
        {
#if ANDROID
            events.AddAndroid(android =>
            {
                android.OnResume(_ => VoipCore.Current.NotifyForeground());
                android.OnPause(_ => VoipCore.Current.NotifyBackground());
            });
#elif IOS
            events.AddiOS(ios =>
            {
                ios.OnActivated(_ => VoipCore.Current.NotifyForeground());
                ios.DidEnterBackground(_ => VoipCore.Current.NotifyBackground());
            });
#endif
        });

        return builder;
    }
}
