namespace Plugin.Maui.VoipCore.Tests;

public sealed class MediaTests
{
    [Fact]
    public async Task Mute_hold_dtmf_and_speaker()
    {
        var (engine, stack, audio, _, _) = Harness.Create();
        await engine.InitializeAsync();
        await engine.RegisterAsync(Harness.Alice);
        var call = await engine.PlaceCallAsync(new CallRequest { Destination = "sip:bob@example.com" });
        stack.ReportCallState(call.Id, CallState.Connected);

        await engine.SetMutedAsync(call.Id, true);
        Assert.True(call.IsMuted);

        await engine.SetHeldAsync(call.Id, true);
        Assert.True(call.IsHeld);
        Assert.Equal(CallState.Held, call.State);

        await engine.SetHeldAsync(call.Id, false);
        Assert.False(call.IsHeld);
        Assert.Equal(CallState.Connected, call.State);

        await engine.SendDtmfAsync(call.Id, "12*#");
        Assert.Equal(["12*#"], stack.SentDtmf);

        await engine.Audio.SetSpeakerAsync(true);
        Assert.True(audio.IsSpeakerOn);
        Assert.Equal(AudioRoute.Speaker, engine.Audio.Route);
    }

    [Fact]
    public async Task Invalid_dtmf_is_rejected()
    {
        var (engine, stack) = await Harness.RegisteredAsync();
        var call = await engine.PlaceCallAsync(new CallRequest { Destination = "sip:bob@example.com" });
        stack.ReportCallState(call.Id, CallState.Connected);

        var ex = await Assert.ThrowsAsync<VoipCoreException>(() => engine.SendDtmfAsync(call.Id, "1x"));

        Assert.Equal(VoipCoreError.InvalidDtmf, ex.Error);
        Assert.Empty(stack.SentDtmf);
    }

    [Fact]
    public async Task Transfer_ends_with_transferred()
    {
        var (engine, stack) = await Harness.RegisteredAsync();
        var call = await engine.PlaceCallAsync(new CallRequest { Destination = "sip:bob@example.com" });
        stack.ReportCallState(call.Id, CallState.Connected);

        await engine.TransferAsync(call.Id, "carol@example.com");

        Assert.Equal(CallState.Disconnected, call.State);
        Assert.Equal(CallEndReason.Transferred, call.EndReason);
    }

    [Fact]
    public async Task Default_speaker_is_applied_on_place_call()
    {
        var (engine, _, audio, _, _) = Harness.Create(o => o.UseSpeakerByDefault = true);
        await engine.InitializeAsync();
        await engine.RegisterAsync(Harness.Alice);

        await engine.PlaceCallAsync(new CallRequest { Destination = "sip:bob@example.com" });

        Assert.True(audio.IsSpeakerOn);
    }
}
