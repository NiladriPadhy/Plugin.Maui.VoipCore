namespace Plugin.Maui.VoipCore.Tests;

public sealed class CallTests
{
    [Fact]
    public async Task Place_call_starts_dialing_and_reports_outgoing()
    {
        var (engine, stack, audio, ui, _) = Harness.Create();
        await engine.InitializeAsync();
        await engine.RegisterAsync(Harness.Alice);

        var call = await engine.PlaceCallAsync(new CallRequest { Destination = "bob@sip.example.com" });

        Assert.Equal(CallState.Dialing, call.State);
        Assert.Equal(CallDirection.Outgoing, call.Direction);
        Assert.Equal("sip:bob@sip.example.com", call.RemoteUri);
        Assert.True(audio.Configured);
        Assert.Contains($"out:{call.Id}", ui.Events);

        stack.ReportCallState(call.Id, CallState.Ringing);
        stack.ReportCallState(call.Id, CallState.Connected);

        Assert.Equal(CallState.Connected, call.State);
        Assert.NotNull(call.ConnectedAt);
        Assert.Same(call, engine.ActiveCall);
        Assert.Contains($"connected:{call.Id}", ui.Events);
    }

    [Fact]
    public async Task Incoming_call_can_be_answered()
    {
        var (engine, stack, audio, ui, _) = Harness.Create();
        await engine.InitializeAsync();
        await engine.RegisterAsync(Harness.Alice);

        IVoipCall? incoming = null;
        engine.IncomingCall += (_, e) => incoming = e.Call;

        var id = stack.SimulateIncoming("carol@sip.example.com", "Carol");
        Assert.NotNull(incoming);
        Assert.Equal(id, incoming!.Id);
        Assert.Equal(CallState.Ringing, incoming.State);
        Assert.Equal(CallDirection.Incoming, incoming.Direction);
        Assert.Contains($"in:{id}", ui.Events);

        var answered = await engine.AnswerAsync(id);

        Assert.Equal(CallState.Connected, answered.State);
        Assert.True(audio.Configured);
    }

    [Fact]
    public async Task Incoming_call_can_be_rejected()
    {
        var (engine, stack) = await Harness.RegisteredAsync();
        var id = stack.SimulateIncoming("sip:dave@example.com");

        await engine.RejectAsync(id, CallRejectReason.Busy);

        var call = engine.GetCall(id);
        Assert.NotNull(call);
        Assert.Equal(CallState.Failed, call!.State);
        Assert.Equal(CallEndReason.Rejected, call.EndReason);
    }

    [Fact]
    public async Task Hangup_disconnects_and_restores_audio()
    {
        var (engine, stack, audio, ui, _) = Harness.Create();
        await engine.InitializeAsync();
        await engine.RegisterAsync(Harness.Alice);
        var call = await engine.PlaceCallAsync(new CallRequest { Destination = "sip:bob@example.com" });
        stack.ReportCallState(call.Id, CallState.Connected);

        await engine.HangupAsync(call.Id);

        Assert.Equal(CallState.Disconnected, call.State);
        Assert.Equal(CallEndReason.LocalHangup, call.EndReason);
        Assert.True(audio.Restored);
        Assert.Contains($"ended:{call.Id}", ui.Events);
        Assert.Null(engine.ActiveCall);
    }

    [Fact]
    public async Task Place_call_requires_registration()
    {
        var (engine, _, _, _, _) = Harness.Create();
        await engine.InitializeAsync();

        var ex = await Assert.ThrowsAsync<VoipCoreException>(() =>
            engine.PlaceCallAsync(new CallRequest { Destination = "sip:bob@example.com" }));

        Assert.Equal(VoipCoreError.NotRegistered, ex.Error);
    }

    [Fact]
    public async Task Concurrent_call_limit_is_enforced()
    {
        var (engine, stack) = await Harness.RegisteredAsync(o => o.MaxConcurrentCalls = 1);
        var first = await engine.PlaceCallAsync(new CallRequest { Destination = "sip:one@example.com" });
        stack.ReportCallState(first.Id, CallState.Connected);

        var ex = await Assert.ThrowsAsync<VoipCoreException>(() =>
            engine.PlaceCallAsync(new CallRequest { Destination = "sip:two@example.com" }));

        Assert.Equal(VoipCoreError.CallLimitReached, ex.Error);
    }

    [Fact]
    public async Task Empty_destination_is_rejected()
    {
        var (engine, _) = await Harness.RegisteredAsync();

        var ex = await Assert.ThrowsAsync<VoipCoreException>(() =>
            engine.PlaceCallAsync(new CallRequest { Destination = "  " }));

        Assert.Equal(VoipCoreError.InvalidDestination, ex.Error);
    }

    [Fact]
    public async Task Unknown_call_id_throws()
    {
        var (engine, _) = await Harness.RegisteredAsync();

        var ex = await Assert.ThrowsAsync<VoipCoreException>(() => engine.HangupAsync("missing"));

        Assert.Equal(VoipCoreError.CallNotFound, ex.Error);
    }
}
