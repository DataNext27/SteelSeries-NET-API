# SteelSeries-NET-API

[![NuGet Downloads](https://img.shields.io/nuget/dt/Steelseries-NET-API?style=for-the-badge&label=Nuget%20Downloads&color=%23004880)](https://www.nuget.org/packages/Steelseries-NET-API)
[![NuGet Version](https://img.shields.io/nuget/vpre/Steelseries-NET-API?style=for-the-badge&label=Version)](https://www.nuget.org/packages/Steelseries-NET-API)
[![GitHub License](https://img.shields.io/github/license/DataNext27/SteelSeries-NET-API?style=for-the-badge&color=red)](https://github.com/DataNext27/SteelSeries-NET-API/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0_%7C_10.0-512cd4?style=for-the-badge)](https://dotnet.microsoft.com/download)
[![Ko-fi](https://img.shields.io/badge/Support_me_on-Ko--fi-FF6433?style=for-the-badge&logo=ko-fi)](https://ko-fi.com/M4M2VL6WW)
> This library is **NOT** affiliated in any way with **SteelSeries**  
> I've made it because it was interesting and funny to do, also I wanted to share this project for people to use it for their own projects

This library allows you to take control over the SteelSeries GG app (only Sonar for now).

The library is available via a [nuget package](https://www.nuget.org/packages/Steelseries-NET-API).  
It is also available in the [Releases](https://github.com/mpaperno/SteelSeries-NET-API/releases) tab as a .zip archive for each supported .NET version.

## Features
- **Full Sonar Control**:
  - **Mixer mode** - read and switch between Classic and Streamer, with confirmation
  - **Volumes & mutes** - per channel (Master, Game, Chat, Media, Aux, Mic), per mix
    (Personal/Stream) in streamer mode
  - **Chat mix** - read and set the game/chat balance
  - **Audio configs** - list presets and custom configs, read and change the selected
    config of each channel
  - **Redirections** - route each channel to a device, toggle channels on the
    streamer mixes, control stream monitoring ("hear what the audience hears")
  - **Audio devices** - list physical and Sonar virtual devices
  - **App routing** - see which application plays on which channel, and move them
  - **Events** - typed .NET events for all of the above, including changes made from
    the Sonar UI, hardware wheels, or Windows volume keys. Automatic reconnection
    when GG restarts.

## Getting Started
Requires Windows with [SteelSeries GG](https://steelseries.com/gg) installed and
Sonar enabled. Targets .NET 8 and .NET 10.
```csharp
using SteelSeriesAPI.Sonar;
using SteelSeriesAPI.Sonar.Enums;

using var sonar = new SonarClient();

// Read and control the mixer
Mode mode = await sonar.Mode.GetAsync();
await sonar.VolumeSettings.SetVolumeAsync(Channel.Game, 0.5);
await sonar.VolumeSettings.SetMuteAsync(Channel.Chat, true);
await sonar.ChatMix.SetAsync(-0.3); // towards game

// List configs and select one
var configs = await sonar.Configs.GetAllAsync(Channel.Game);
await sonar.Configs.SelectAsync(configs[0].Id);

// Route an application to another channel
await sonar.AppRouting.RouteAppAsync(processId: 7244, Channel.Media);
```

### Listening to changes

```csharp
sonar.Events.PollingInterval = TimeSpan.FromMilliseconds(500); // enables full detection
sonar.Events.VolumeChanged += (_, e) =>
    Console.WriteLine($"{e.Channel}: {e.PreviousVolume:P0} -> {e.NewVolume:P0}");
sonar.Events.ChatMixChanged += (_, e) =>
    Console.WriteLine($"ChatMix balance: {e.Balance}");
sonar.Events.ModeChanged += (_, e) =>
    Console.WriteLine($"Mode: {e.PreviousMode} -> {e.NewMode}");

sonar.Events.Start();   // no admin rights needed
// ...
await sonar.Events.StopAsync();
```
Events cover connection lifecycle, volumes, mode, chat mix, redirections, configs,
and application audio sessions. See
[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the full event table and how
detection works under the hood.

The [Sample project](SteelSeriesAPI.Sample/Program.cs) is a complete demo: it dumps
the current Sonar state, then prints every event live.

### Error handling

Everything the library throws derives from `SteelSeriesException`:

```csharp
try
{
    await sonar.Mode.SetAsync(Mode.Streamer);
}
catch (SonarWrongModeException) { /* operation not available in this mode */ }
catch (SteelSeriesException e)  { /* GG not running, Sonar disabled, API changed... */ }
```

The client self-heals across GG restarts (the Sonar server changes port every
time): addresses are rediscovered and the event stream reconnects automatically.

## Migrating from 1.x

Version 2.0 is a full rewrite; the API surface changed. The essentials:

| V1 | V2 |
|---|---|
| `new SonarBridge()` | `new SonarClient()` |
| `WaitUntilSteelSeriesStarted()` / `WaitUntilSonarStarted()` | Not needed - discovery is automatic, failures throw typed exceptions |
| Synchronous calls (`Mode.Get()`) | Async end to end (`await Mode.GetAsync()`) |
| `Device` enum | `Channel` enum |
| `StartListener()` **(admin rights)** | `Events.Start()` **(no admin)** |
| `SonarEventManager.OnSonarModeChange` | `Events.ModeChanged` (typed args with previous/new values) |

Why the rewrite? V1 detected changes by sniffing network packets (hence admin
rights) and crashed whenever GG updates changed a field. V2 uses Sonar's own
WebSocket plus light polling, parses tolerantly (unknown fields and channels are
skipped, never fatal), and ships tooling to catch SteelSeries-side changes early.

## For contributors

- [ARCHITECTURE.md](docs/ARCHITECTURE.md) - layers, event mechanisms, and the
  hard-earned lessons about the Sonar API
- [CONTRIBUTING.md](docs/CONTRIBUTING.md) - setup, the manager mold, and the
  post-GG-update checklist
- `SteelSeriesAPI.Explorer` - the exploration tool this library was built with:
  probe routes, dump payloads, `check` API structures against references, `verify`
  write contracts live

## Todo
(Actually not planned as not possible, maybe one day I guess :/ )
- Moments
- Engine
- Settings

If anyone find a way to control these above, feel free to create a pull request or an issue

## Projects using this library
- [TouchPortal SteelSeries GG Plugin](https://github.com/DataNext27/TouchPortal_SteelSeriesGG) by DataNext
