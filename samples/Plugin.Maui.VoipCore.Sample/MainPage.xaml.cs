using Plugin.Maui.VoipCore;

namespace Plugin.Maui.VoipCore.Sample;

public partial class MainPage : ContentPage
{
    readonly IVoipCore _voip;

    public MainPage(IVoipCore voip)
    {
        InitializeComponent();
        _voip = voip;
        _voip.RegistrationChanged += (_, e) => MainThread.BeginInvokeOnMainThread(Refresh);
        _voip.CallChanged += (_, _) => MainThread.BeginInvokeOnMainThread(Refresh);
        _voip.IncomingCall += (_, e) => MainThread.BeginInvokeOnMainThread(() =>
            StatusLabel.Text = $"Incoming from {e.Call.DisplayName ?? e.Call.RemoteUri}.");
        Refresh();
    }

    async void OnRegisterClicked(object? sender, EventArgs e)
        => await RunAsync("Register", async () =>
        {
            await _voip.InitializeAsync();
            await _voip.RegisterAsync(new SipAccount
            {
                Username = UserEntry.Text?.Trim() ?? "alice",
                Domain = DomainEntry.Text?.Trim() ?? "sip.example.com",
                Password = string.IsNullOrWhiteSpace(PasswordEntry.Text) ? null : PasswordEntry.Text
            });
            return $"Registered as {_voip.Account?.Aor} via {_voip.Stack.Name}.";
        });

    async void OnUnregisterClicked(object? sender, EventArgs e)
        => await RunAsync("Unregister", async () =>
        {
            await _voip.UnregisterAsync();
            return "Unregistered.";
        });

    async void OnCallClicked(object? sender, EventArgs e)
        => await RunAsync("Call", async () =>
        {
            await _voip.EnsureMicrophoneAsync();
            var call = await _voip.PlaceCallAsync(new CallRequest
            {
                Destination = DestinationEntry.Text ?? "bob@sip.example.com"
            });
            return $"Dialing {call.RemoteUri} ({call.Id}).";
        });

    async void OnSimulateIncomingClicked(object? sender, EventArgs e)
        => await RunAsync("Incoming", () =>
        {
            if (_voip.Stack is not LoopbackSipStack loopback)
            {
                return Task.FromResult("Simulate incoming is available on LoopbackSipStack.");
            }

            var id = loopback.SimulateIncoming("sip:carol@sip.example.com", "Carol");
            return Task.FromResult($"Simulated incoming {id}.");
        });

    async void OnAnswerClicked(object? sender, EventArgs e)
        => await RunAsync("Answer", async () =>
        {
            var call = RequireActive();
            await _voip.AnswerAsync(call.Id);
            return $"Answered {call.RemoteUri}.";
        });

    async void OnRejectClicked(object? sender, EventArgs e)
        => await RunAsync("Reject", async () =>
        {
            var call = RequireActive();
            await _voip.RejectAsync(call.Id);
            return $"Rejected {call.RemoteUri}.";
        });

    async void OnHangupClicked(object? sender, EventArgs e)
        => await RunAsync("Hangup", async () =>
        {
            var call = RequireActive();
            await _voip.HangupAsync(call.Id);
            return $"Hung up {call.RemoteUri}.";
        });

    async void OnMuteClicked(object? sender, EventArgs e)
        => await RunAsync("Mute", async () =>
        {
            var call = RequireActive();
            await _voip.SetMutedAsync(call.Id, !call.IsMuted);
            return call.IsMuted ? "Muted." : "Unmuted.";
        });

    async void OnHoldClicked(object? sender, EventArgs e)
        => await RunAsync("Hold", async () =>
        {
            var call = RequireActive();
            await _voip.SetHeldAsync(call.Id, !call.IsHeld);
            return call.IsHeld ? "On hold." : "Resumed.";
        });

    async void OnSpeakerClicked(object? sender, EventArgs e)
        => await RunAsync("Speaker", async () =>
        {
            await _voip.Audio.SetSpeakerAsync(!_voip.Audio.IsSpeakerOn);
            return _voip.Audio.IsSpeakerOn ? "Speaker on." : "Earpiece.";
        });

    async void OnDtmfClicked(object? sender, EventArgs e)
    {
        var digit = (sender as Button)?.CommandParameter as string ?? (sender as Button)?.Text;
        if (string.IsNullOrEmpty(digit))
        {
            return;
        }

        await RunAsync("DTMF", async () =>
        {
            var call = RequireActive();
            await _voip.SendDtmfAsync(call.Id, digit);
            return $"Sent DTMF {digit}.";
        });
    }

    IVoipCall RequireActive() =>
        _voip.ActiveCall ?? throw new VoipCoreException(VoipCoreError.CallNotFound, "No active call.");

    async Task RunAsync(string action, Func<Task<string>> work)
    {
        try
        {
            StatusLabel.Text = await work();
            Refresh();
        }
        catch (VoipCoreException ex)
        {
            StatusLabel.Text = $"{action} failed ({ex.Error}): {ex.Message}";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"{action} failed: {ex.Message}";
        }
    }

    void Refresh()
    {
        EngineLabel.Text = $"Engine: {_voip.State}  |  Reg: {_voip.Registration}  |  Stack: {_voip.Stack.Name}";
        var call = _voip.ActiveCall;
        CallLabel.Text = call is null
            ? "No active call."
            : $"{call.Direction} {call.RemoteUri}  {call.State}  mute={call.IsMuted}  hold={call.IsHeld}";
    }
}
