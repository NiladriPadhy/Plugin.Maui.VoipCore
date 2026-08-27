namespace Plugin.Maui.VoipCore.Tests;

public sealed class RegistrationTests
{
    [Fact]
    public async Task Register_sets_registered_state()
    {
        var (engine, _, _, _, _) = Harness.Create();
        await engine.InitializeAsync();

        await engine.RegisterAsync(Harness.Alice);

        Assert.Equal(VoipEngineState.Registered, engine.State);
        Assert.Equal(RegistrationState.Registered, engine.Registration);
        Assert.Equal("alice", engine.Account?.Username);
    }

    [Fact]
    public async Task Register_raises_events()
    {
        var states = new List<RegistrationState>();
        var (engine, _, _, _, _) = Harness.Create(o => o.Events.OnRegistrationChanged = e => states.Add(e.State));
        await engine.InitializeAsync();

        await engine.RegisterAsync(Harness.Alice);

        Assert.Contains(RegistrationState.Registering, states);
        Assert.Contains(RegistrationState.Registered, states);
    }

    [Fact]
    public async Task Unregister_returns_to_ready()
    {
        var (engine, _) = await Harness.RegisteredAsync();

        await engine.UnregisterAsync();

        Assert.Equal(VoipEngineState.Ready, engine.State);
        Assert.Equal(RegistrationState.Unregistered, engine.Registration);
    }

    [Fact]
    public async Task Failed_registration_throws()
    {
        var (engine, _, _, _, _) = Harness.Create(configureStack: s =>
        {
            s.FailRegistration = true;
            s.FailRegistrationMessage = "403 Forbidden";
        });
        await engine.InitializeAsync();

        var ex = await Assert.ThrowsAsync<VoipCoreException>(() => engine.RegisterAsync(Harness.Alice));

        Assert.Equal(VoipCoreError.RegistrationFailed, ex.Error);
        Assert.Equal(RegistrationState.Failed, engine.Registration);
        Assert.Equal(VoipEngineState.Failed, engine.State);
    }

    [Fact]
    public async Task Invalid_account_is_rejected()
    {
        var (engine, _, _, _, _) = Harness.Create();
        await engine.InitializeAsync();

        var ex = await Assert.ThrowsAsync<VoipCoreException>(() =>
            engine.RegisterAsync(new SipAccount { Username = " ", Domain = "sip.example.com" }));

        Assert.Equal(VoipCoreError.InvalidAccount, ex.Error);
    }

    [Fact]
    public async Task Auto_register_on_initialize()
    {
        var (engine, _, _, _, _) = Harness.Create(o =>
        {
            o.Account = Harness.Alice;
            o.AutoRegister = true;
        });

        await engine.InitializeAsync();

        Assert.Equal(RegistrationState.Registered, engine.Registration);
    }
}
