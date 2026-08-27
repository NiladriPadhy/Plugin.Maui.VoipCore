namespace Plugin.Maui.VoipCore.Tests;

sealed class FakeClock : IClock
{
    public DateTimeOffset UtcNow { get; set; } = new(2026, 8, 28, 6, 40, 0, TimeSpan.Zero);

    public void Advance(TimeSpan duration) => UtcNow += duration;
}

sealed class MemoryAudioSession : IVoipAudioSession
{
    public bool Configured { get; private set; }

    public bool Restored { get; private set; }

    public bool IsSpeakerOn { get; private set; }

    public AudioRoute Route => IsSpeakerOn ? AudioRoute.Speaker : AudioRoute.Earpiece;

    public Task ConfigureForCallAsync(CancellationToken cancellationToken)
    {
        Configured = true;
        Restored = false;
        return Task.CompletedTask;
    }

    public Task SetSpeakerAsync(bool enabled, CancellationToken cancellationToken)
    {
        IsSpeakerOn = enabled;
        return Task.CompletedTask;
    }

    public Task RestoreAsync(CancellationToken cancellationToken)
    {
        Restored = true;
        Configured = false;
        IsSpeakerOn = false;
        return Task.CompletedTask;
    }
}

sealed class RecordingCallUi : IVoipCallUi
{
    public List<string> Events { get; } = [];

    public void Attach(IVoipCallUiHandler handler) => Events.Add("attach");

    public Task ReportOutgoingAsync(IVoipCall call, CancellationToken cancellationToken)
    {
        Events.Add($"out:{call.Id}");
        return Task.CompletedTask;
    }

    public Task ReportIncomingAsync(IVoipCall call, CancellationToken cancellationToken)
    {
        Events.Add($"in:{call.Id}");
        return Task.CompletedTask;
    }

    public Task ReportConnectedAsync(IVoipCall call, CancellationToken cancellationToken)
    {
        Events.Add($"connected:{call.Id}");
        return Task.CompletedTask;
    }

    public Task ReportEndedAsync(IVoipCall call, CancellationToken cancellationToken)
    {
        Events.Add($"ended:{call.Id}");
        return Task.CompletedTask;
    }
}

static class Harness
{
    public static SipAccount Alice { get; } = new()
    {
        Username = "alice",
        Domain = "sip.example.com",
        Password = "secret"
    };

    public static (VoipCoreImplementation Engine, LoopbackSipStack Stack, MemoryAudioSession Audio, RecordingCallUi Ui, FakeClock Clock) Create(
        Action<VoipCoreOptions>? configure = null,
        Action<LoopbackSipStackOptions>? configureStack = null)
    {
        var stackOptions = new LoopbackSipStackOptions { AutoProgress = false };
        configureStack?.Invoke(stackOptions);
        var stack = new LoopbackSipStack(stackOptions);
        var audio = new MemoryAudioSession();
        var ui = new RecordingCallUi();
        var clock = new FakeClock();
        var options = new VoipCoreOptions();
        configure?.Invoke(options);
        options.StackFactory = () => stack;

        var engine = VoipCore.Create(options, stack, audio, ui, clock);
        return (engine, stack, audio, ui, clock);
    }

    public static async Task<(VoipCoreImplementation Engine, LoopbackSipStack Stack)> RegisteredAsync(
        Action<VoipCoreOptions>? configure = null)
    {
        var (engine, stack, _, _, _) = Create(configure);
        await engine.InitializeAsync();
        await engine.RegisterAsync(Alice);
        return (engine, stack);
    }
}
