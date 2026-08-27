namespace Plugin.Maui.VoipCore;

/// <summary>
/// Entry point for the VoipCore plugin when dependency injection is not used.
/// </summary>
public static class VoipCore
{
    static IVoipCore? _current;

    /// <summary>
    /// Gets the shared <see cref="IVoipCore"/> instance.
    /// </summary>
    public static IVoipCore Current => _current ??= Create(new VoipCoreOptions());

    /// <summary>
    /// Creates an engine with the default loopback stack, or <see cref="VoipCoreOptions.StackFactory"/>.
    /// </summary>
    public static IVoipCore Create(VoipCoreOptions? options = null)
    {
        var resolved = options ?? new VoipCoreOptions();
        var stack = resolved.StackFactory?.Invoke() ?? new LoopbackSipStack();
        return new VoipCoreImplementation(
            resolved,
            stack,
            new PlatformAudioSession(),
            new PlatformCallUi(resolved),
            SystemClock.Instance);
    }

    /// <summary>
    /// Replaces the shared instance. Intended for tests and custom implementations.
    /// </summary>
    public static void SetDefault(IVoipCore implementation) =>
        _current = implementation ?? throw new ArgumentNullException(nameof(implementation));

    internal static VoipCoreImplementation Create(
        VoipCoreOptions options,
        ISipStack stack,
        IVoipAudioSession audio,
        IVoipCallUi callUi,
        IClock clock) =>
        new(options, stack, audio, callUi, clock);
}
