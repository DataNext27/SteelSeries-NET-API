# Architecture

This document explains how the library is structured, how change detection works,
and the hard-earned lessons about the Sonar API that shaped the design. Read this
before touching the internals: most "weird" choices in the code are direct
consequences of how the Sonar API actually behaves.

## The big picture

The Sonar API is an undocumented, local HTTP + WebSocket API embedded in SteelSeries GG.
This library wraps it in three layers:

SteelSeriesAPI/  
├── Core/ "How to talk to Sonar" - discovery, HTTP, exceptions, JSON helpers  
├── Sonar/  
│ ├── Managers/ "Controlling Sonar" - typed read/write operations, on demand  
│ ├── Events/ "Being told of changes" - WebSocket + polling, typed .NET events  
│ ├── Models/ - immutable records returned by managers and events  
│ ├── Enums/ - Channel, Mix, Mode, AudioDataFlow + API vocabulary mappings  
│ ├── SonarClient.cs - the single entry point, wires everything together  
│ └── SonarRoutes.cs - every HTTP route, centralized  
└── (tooling: SteelSeriesAPI.Explorer, SteelSeriesAPI.Sample, SteelSeriesAPI.Tests)  


**Dependency rule:** Managers and Events depend on `Core` through the
`ISonarTransport` interface, never on each other's internals. `SonarClient` is the
only place that knows the concrete types and wires them together.

### Core

- `ServerDiscovery` resolves the Sonar web server address:
  `coreProps.json` → GG `/subApps` endpoint (HTTPS, self-signed cert) → Sonar base URL.
  The port changes on every GG restart, so the address is never assumed stable.
- `SonarHttpClient` owns the single `HttpClient`, caches the resolved address, and
  self-heals: on a transport failure it invalidates the cache, rediscovers, and
  retries once. It implements `ISonarTransport`, which is what everything else
  consumes - and what tests fake.
- `SonarExceptions` is the exception taxonomy. Everything the library throws derives
  from `SteelSeriesException`, so consumers can catch one type. `SonarWrongModeException`
  and `SonarRequestException` (with `StatusCode`/`ResponseBody`) carry diagnostics.

### Managers

One manager per functional domain (volumes, mode, chat mix, redirections, devices,
configs, app routing). The pattern is always the same:

- a public interface (`IVolumeSettingsManager`) next to an `internal sealed` class,
- the class receives `ISonarTransport`, nothing else,
- routes live in `SonarRoutes`, never inline,
- parsing is a pure `internal static` function, testable against real payloads,
- validation happens before any HTTP call.

### Events

`SonarEventListener` raises typed .NET events fed by three mechanisms. Subscribers
never know (or care) which mechanism produced an event.

| Event                                                                                                          | Mechanism |
|----------------------------------------------------------------------------------------------------------------|---|
| `Connected` / `Disconnected`                                                                                   | WebSocket connection lifecycle |
| `ChatMixChanged`                                                                                               | WebSocket broadcast (real time) |
| `VolumeDataReceived`                                                                                           | WebSocket broadcast (connection, mode switch, OS/hardware changes) |
| `AudioSessionOpened` / `AudioSessionClosed`                                                                    | WebSocket broadcast |
| `*Invalidated` (redirections, configs, routing)                                                                | WebSocket broadcast (raw, no data) |
| `UnknownEventReceived`                                                                                         | WebSocket broadcast (catch-all) |
| `VolumeChanged`                                                                                                | Polling (mode-aware diff) |
| `ModeChanged`                                                                                                  | Polling |
| `ClassicDeviceChanged`, `MixDeviceChanged`, `MixChannelToggled`, `StreamMonitoringChanged`, `MicDeviceChanged` | Hybrid: invalidation + polling → fetch + diff |
| `ConfigSelectionChanged`                                                                                       | Hybrid: invalidation + polling → fetch + diff |

**Why three mechanisms?** Because Sonar's WebSocket (`/sock`) only broadcasts what
does *not* come through its own HTTP API. UI slider moves, mix toggles, and config
selections are HTTP writes from the GG UI, so the server never re-broadcasts them.
Polling fills that gap. The hybrid pattern (invalidation → debounced fetch → diff →
granular events) turns Sonar's empty `data: null` signals into rich events.

Key internals:

- The WebSocket loop reconnects automatically with exponential backoff (1s→30s).
  A dead connection *is* the "GG closed" detector; on reconnection Sonar re-pushes
  its full state, so subscribers resynchronize for free.
- `DebouncedRefresher` collapses invalidation bursts into a single fetch (250ms
  debounce) and serializes refreshes with the polling tick.
- Diff baselines are only compared within the same mode (see stale sections below).
- `RaiseSafely` guarantees a throwing subscriber never kills a background loop.

## Lessons about the Sonar API

These are empirical findings, each one shaped the code. Dates refer to when they
were observed; the Explorer's `check`/`verify` commands guard them.

1. **The port changes on every GG restart.** Never cache the address beyond a
   connection failure. (`SonarHttpClient` invalidation + rediscovery)
2. **Writes are rejected in the wrong mode** with HTTP 500
   `"Cannot be called in current mode"`. (`SonarWrongModeException`)
3. **Stale sections:** each `volumeSettings/{mode}` route only reliably reflects
   its *own* mode's values. The other mode's sections return stale data. Poll the
   route matching the current mode; never diff across modes. (2026-08-08)
4. **The server does not re-broadcast its own HTTP writes.** UI slider moves,
   mix toggles, config selections: invisible on the WebSocket. Hence polling.
5. **Three channel vocabularies** coexist: JSON keys (`chatRender`/`chatCapture`),
   route keys (capitalized `Volume`/`Mute` in classic, lowercase in streamer), and
   classic redirection ids (`chat`/`mic`). All mappings live in `ChannelExtensions`.
6. **Device GUIDs are regenerated by GG updates** and **process ids change on every
   launch**. Never persist either; resolve at call time. (`RouteAppAsync` does.)
7. **`configs` weighs >1MB** (every preset embeds its full EQ twice). Never poll it;
   parse headers lazily. `configs/selected` is the cheap alternative.
8. **Routes are being migrated to a `/v1/` prefix** (chatMix moved in a 2026 update,
   breaking the unprefixed route). Expect more; `SonarRoutes` centralizes the fix.
9. **Event names are inconsistent** (`EVENT_SONAR_*` vs `SONAR_EVENT_*`): SteelSeries'
   doing, centralized in `SonarEventNames`.
10. **Float noise** (`0.45999998`): compare records by value, avoid strict equality
    on raw doubles in new code.

## Design invariants

When contributing, preserve these:

- **Minimal tolerant parsing.** `TryGetProperty` + `ValueKind` checks everywhere.
  Unknown fields, channels, or entries are *skipped*, never fatal. A GG update must
  degrade features, not crash consumers. (This killed V1.)
- **Only parse what the library exposes.** Don't materialize fields "in case".
- **Routes and event names are centralized** (`SonarRoutes`, `SonarEventNames`),
  commented and dated.
- **Wrap at the boundary:** everything thrown to consumers derives from
  `SteelSeriesException`, with the underlying exception as `InnerException`.
- **Async end to end** with `CancellationToken` on every public method.
- **Records for models:** value equality is what makes the diffing cheap.
- **`InvariantCulture` for every number formatted into a route** (French machines
  write `0,5` otherwise).
- **Test fixtures are real captured payloads**, dated in a comment, trimmed but
  never hand-idealized.