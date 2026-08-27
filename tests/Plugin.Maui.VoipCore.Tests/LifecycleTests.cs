namespace Plugin.Maui.VoipCore.Tests;

public sealed class LifecycleTests
{
    [Fact]
    public async Task Register_initializes_automatically()
    {
        var (engine, _, _, _, _) = Harness.Create();

        await engine.RegisterAsync(Harness.Alice);

        Assert.True(engine.IsInitialized);
        Assert.Equal(RegistrationState.Registered, engine.Registration);
    }

    [Fact]
    public async Task Place_call_after_shutdown_requires_initialize()
    {
        var (engine, _) = await Harness.RegisteredAsync();
        await engine.ShutdownAsync();

        var ex = await Assert.ThrowsAsync<VoipCoreException>(() =>
            engine.PlaceCallAsync(new CallRequest { Destination = "sip:bob@example.com" }));

        Assert.Equal(VoipCoreError.NotInitialized, ex.Error);
    }

    [Fact]
    public async Task Shutdown_hangs_up_and_resets()
    {
        var (engine, stack, audio, _, _) = Harness.Create();
        await engine.InitializeAsync();
        await engine.RegisterAsync(Harness.Alice);
        var call = await engine.PlaceCallAsync(new CallRequest { Destination = "sip:bob@example.com" });
        stack.ReportCallState(call.Id, CallState.Connected);

        await engine.ShutdownAsync();

        Assert.False(engine.IsInitialized);
        Assert.Equal(VoipEngineState.Idle, engine.State);
        Assert.Equal(RegistrationState.None, engine.Registration);
        Assert.True(call.IsTerminal);
        Assert.True(audio.Restored);
    }

    [Fact]
    public async Task Background_holds_connected_calls()
    {
        var (engine, stack) = await Harness.RegisteredAsync(o => o.HoldOnBackground = true);
        var call = await engine.PlaceCallAsync(new CallRequest { Destination = "sip:bob@example.com" });
        stack.ReportCallState(call.Id, CallState.Connected);

        engine.NotifyBackground();
        await Task.Delay(50);

        Assert.Equal(CallState.Held, call.State);

        engine.NotifyForeground();
        await Task.Delay(50);

        Assert.Equal(CallState.Connected, call.State);
    }

    [Fact]
    public async Task Call_duration_uses_clock()
    {
        var (engine, stack, _, _, clock) = Harness.Create();
        await engine.InitializeAsync();
        await engine.RegisterAsync(Harness.Alice);
        var call = await engine.PlaceCallAsync(new CallRequest { Destination = "sip:bob@example.com" });
        stack.ReportCallState(call.Id, CallState.Connected);

        clock.Advance(TimeSpan.FromMinutes(2));

        Assert.Equal(TimeSpan.FromMinutes(2), call.Duration);
    }

    [Fact]
    public void Sip_uri_and_dtmf_helpers()
    {
        Assert.Equal("sip:bob@example.com", SipUri.Normalize("bob@example.com"));
        Assert.Equal("sips:alice@example.com", SipUri.Normalize("sips:alice@example.com"));
        Assert.True(SipUri.IsValidDtmf("9*#A"));
        Assert.False(SipUri.IsValidDtmf("9x"));
        Assert.Throws<VoipCoreException>(() => SipUri.Normalize(" "));
    }

    [Fact]
    public void Account_builds_sip_uri()
    {
        var tls = new SipAccount
        {
            Username = "alice",
            Domain = "example.com",
            Transport = SipTransport.Tls
        };

        Assert.Equal("alice@example.com", tls.Aor);
        Assert.Equal("sips:alice@example.com", tls.SipUri);
        Assert.Equal(5061, tls.EffectivePort);
    }
}
