# Changelog

## 1.0.0

- Generic SIP/VoIP abstraction for .NET MAUI on iOS and Android
- Account registration, outgoing/incoming calls, hold, mute, speaker, DTMF, and transfer
- Pluggable `ISipStack` with `ISipStackSink` events
- Built-in `LoopbackSipStack` for tests and UI development
- iOS `AVAudioSession` voice-chat routing and optional CallKit reporting
- Android `AudioManager` in-communication mode and speaker control
- Hold-on-background and resume lifecycle hooks
- .NET MAUI support for iOS and Android (`net10.0-ios`, `net10.0-android`)
