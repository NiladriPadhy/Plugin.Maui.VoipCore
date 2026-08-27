namespace Plugin.Maui.VoipCore;

/// <summary>
/// Registers VoipCore services without MAUI lifecycle hooks.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="IVoipCore"/> using the supplied options instance.
    /// </summary>
    public static IServiceCollection AddVoipCore(this IServiceCollection services, VoipCoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        services.AddSingleton(options);
        services.TryAddSingleton<IVoipCore>(sp =>
        {
            var engine = VoipCore.Create(options);
            VoipCore.SetDefault(engine);
            return engine;
        });

        return services;
    }

    /// <summary>
    /// Adds <see cref="IVoipCore"/> and applies <paramref name="configure"/> to a new options instance.
    /// </summary>
    public static IServiceCollection AddVoipCore(this IServiceCollection services, Action<VoipCoreOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new VoipCoreOptions();
        configure?.Invoke(options);
        return services.AddVoipCore(options);
    }
}
