# Plugin.Maui.VoipCore

[![NuGet](https://img.shields.io/nuget/v/Plugin.Maui.VoipCore.svg?label=NuGet)](https://www.nuget.org/packages/Plugin.Maui.VoipCore)

Generic SIP/VoIP abstraction for **.NET MAUI** on **iOS** and **Android**.

The package owns the session model (register, call, hold, mute, speaker, DTMF) and platform audio. Signaling and media come from a pluggable `ISipStack` — ship your own PJSIP/Linphone adapter, or use the built-in loopback stack for tests and UI work.

| Feature | What it does |
| --- | --- |
| **Accounts** | SIP username, domain, transport (UDP/TCP/TLS), proxy, STUN, ICE |
| **Calls** | Place, answer, reject, hangup, transfer, concurrent-call limit |
| **Media** | Mute, hold, speaker route, DTMF (`0-9*#A-D`) |
| **Platform audio** | iOS `AVAudioSession` voice-chat; Android `AudioManager` in-communication |
| **Call UI** | Optional iOS CallKit reporting; Android uses in-app `IncomingCall` |
| **Pluggable stack** | `ISipStack` + `ISipStackSink` so a native SIP engine can be swapped in |
| **Loopback** | In-process stack with `SimulateIncoming` for samples and unit tests |

## Install

Package: [https://www.nuget.org/packages/Plugin.Maui.VoipCore](https://www.nuget.org/packages/Plugin.Maui.VoipCore)

```bash
dotnet add package Plugin.Maui.VoipCore
```

## Quick start

```csharp
using Plugin.Maui.VoipCore;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseVoipCore(options =>
            {
                options.Account = new SipAccount
                {
                    Username = "alice",
                    Domain = "sip.example.com",
                    Password = "secret",
                    Transport = SipTransport.Udp
                };
                options.HoldOnBackground = true;
            });

        return builder.Build();
    }
}
```

Resolve `IVoipCore` or use `VoipCore.Current`:

```csharp
var voip = handler.Services.GetRequiredService<IVoipCore>();

await voip.InitializeAsync();
await voip.RegisterAsync(new SipAccount
{
    Username = "alice",
    Domain = "sip.example.com"
});

voip.IncomingCall += (_, e) => { /* show answer UI */ };

var call = await voip.PlaceCallAsync(new CallRequest
{
    Destination = "bob@sip.example.com",
    DisplayName = "Bob"
});

await voip.SetMutedAsync(call.Id, true);
await voip.SendDtmfAsync(call.Id, "123#");
await voip.HangupAsync(call.Id);
```

The default stack is `LoopbackSipStack` (no network). Outgoing loopback calls connect after `PlaceCallAsync` returns; inject inbound calls with `SimulateIncoming`.

## Bring your own SIP stack

Implement `ISipStack` and report events through `ISipStackSink`:

```csharp
builder.UseVoipCore(options =>
{
    options.StackFactory = () => new MyPjsipStack();
});
```

`InitializeAsync` receives a `SipStackContext` with the sink and options. Raise `OnRegistrationChanged`, `OnIncomingCall`, and `OnCallStateChanged` as the native engine reports them.

## Loopback (tests and sample)

```csharp
var stack = new LoopbackSipStack(new LoopbackSipStackOptions
{
    AutoProgress = false
});

var voip = VoipCore.Create(new VoipCoreOptions
{
    StackFactory = () => stack
});

await voip.InitializeAsync();
await voip.RegisterAsync(account);

var call = await voip.PlaceCallAsync(new CallRequest { Destination = "sip:bob@example.com" });
stack.ReportCallState(call.Id, CallState.Connected);

stack.SimulateIncoming("sip:carol@example.com", "Carol");
```

## Lifecycle hooks

`UseVoipCore` wires platform resume/pause:

- **Background** — hold connected calls when `HoldOnBackground` is `true`
- **Resume** — resume calls that were held for backgrounding

```csharp
voip.NotifyForeground();
voip.NotifyBackground();
```

## Without the generic host

```csharp
var voip = VoipCore.Create(new VoipCoreOptions
{
    UseSpeakerByDefault = true
});

await voip.InitializeAsync();
```

## Platform notes

**iOS** — set `NSMicrophoneUsageDescription` and `UIBackgroundModes` (`audio`, `voip`) in `Info.plist`. When `UseNativeCallUi` is `true`, incoming/outgoing calls are reported to CallKit.

**Android** — declare `RECORD_AUDIO`, `MODIFY_AUDIO_SETTINGS`, `INTERNET`, and `ACCESS_NETWORK_STATE`. Incoming calls are delivered through `IncomingCall`; host a `ConnectionService` in the app if you need Telecom UI.

## Target frameworks

The package targets `net10.0`, `net10.0-android`, and `net10.0-ios`.

## Pack from source

```bash
dotnet pack src/Plugin.Maui.VoipCore/Plugin.Maui.VoipCore.csproj -c Release -o artifacts
```

The `.nupkg` is written to `artifacts/Plugin.Maui.VoipCore.1.0.0.nupkg`.

## License

MIT

## Support

> If this plugin saved you a weekend of native plumbing, consider buying me a coffee.
> Your support keeps it maintained, documented, and free.

[![Buy Me A Coffee](https://img.shields.io/badge/Buy%20Me%20a%20Coffee-ffdd00?style=for-the-badge&logo=buy-me-a-coffee&logoColor=black)](https://buymeacoffee.com/npadhy)

This library stays open source. A coffee helps cover time for bug fixes, new features, and docs.
