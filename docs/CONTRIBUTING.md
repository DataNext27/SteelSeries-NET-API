# Contributing

Thanks for your interest! This project reverse-engineers the local SteelSeries
Sonar API, so contributing has two sides: regular C# work, and careful verification
against the real API. This guide covers both.

Start by reading [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) - most design
questions are answered there.

## Setup

- .NET SDK 10 (the library multi-targets `net8.0;net10.0`)
- Windows with SteelSeries GG installed and Sonar enabled - *only needed for live
  testing*: the unit test suite runs anywhere, without GG
- Solution layout:
    - `SteelSeriesAPI` - the library (the only shipped project)
    - `SteelSeriesAPI.Tests` - xUnit tests, no network, run everywhere
    - `SteelSeriesAPI.Sample` - manual test bench: dumps the full state, then prints every event live
    - `SteelSeriesAPI.Explorer` - API exploration & verification tool (see below)

```bash
dotnet build
dotnet test          # must stay green on net8.0 and net10.0
dotnet run --project SteelSeriesAPI.Sample     # requires GG running
dotnet run --project SteelSeriesAPI.Explorer   # requires GG running
```

## The Explorer

The Explorer is how this library was built and how it stays alive across GG updates.

| Command | Purpose |
|---|---|
| `get <route>` / `put <route>` | Play with any route by hand |
| `probe <r1> <r2>...` | Test route candidates (distinguishes 404 from wrong-mode 500) |
| `ws [path]` | Listen to a WebSocket and print every message |
| `dump` | Snapshot every known GET route into `dumps/<date>/` |
| `check` | Compare response *structures* against committed references (`reference-shapes/`) |
| `check update` | Re-capture the references (do this only on a version you trust) |
| `verify` | Live write round-trips: set, read back, restore, for every write route |

`check` and `verify` handle the mixer mode themselves and restore your state.

## After a GG update (the checklist)

GG updates are what break this kind of library. When one lands:

1. `dotnet run --project SteelSeriesAPI.Explorer -- check`
    - `STRUCTURE CHANGED`? Diff the `.shape.json` vs `.shape.json.actual` files:
      that's exactly what SteelSeries changed.
2. `dotnet run --project SteelSeriesAPI.Explorer -- verify`
    - a `FAIL` means a write contract broke (renamed route, new vocabulary...).
3. `dump`, and diff against the previous dated folder for the human-readable view.
4. If something broke: fix `SonarRoutes`/parsers, update the test fixtures with
   freshly captured payloads (keep the capture date comment), run `check update`,
   and commit the new references with the fix.
5. Nothing broke? Enjoy, you just spent 30 seconds.

## Adding a manager (the mold)

Every manager follows the same shape - copy an existing one (e.g. `ChatMixManager`
for a simple one, `RedirectionsManager` for a rich one):

1. **Explore first.** Find the routes (Explorer `probe`, Wireshark on loopback, or
   grepping GG's `app.asar`), capture real payloads, verify writes by hand.
2. `SonarRoutes`: add the routes, commented and dated ("Verified live on ...").
3. `Models/`: immutable records, only the fields the library exposes.
4. `Managers/`: public interface + `internal sealed` class taking `ISonarTransport`.
   Parsing goes in a pure `internal static` method. Validate before sending.
5. `SonarClient`: expose the interface, instantiate in the constructor.
6. **Tests**: fixtures = your captured payloads (dated), plus the standard cases:
   real payload parses, unknown entries are skipped without crashing, write routes
   build exactly the verified strings, validation throws *before* any HTTP call.
7. Sample: add the state dump and event subscriptions if relevant.
8. If the domain needs change detection, follow the hybrid pattern in
   `SonarEventListener.Redirections.cs` (it reuses `DebouncedRefresher`).

## Tests

- Unit tests run against `FakeTransport` with real captured payloads as fixtures.
  They protect *this library's code*; they cannot detect SteelSeries-side changes -
  that's the Explorer's `check`/`verify` job.
- Keep the suite green on both target frameworks; CI runs it on every push/PR.
- New fixtures: paste the real payload (trim huge blobs like EQ data), date it.

## Style

- C# 12, nullable enabled, XML docs on every public member (English).
- Comments explain *why*, and carry dates when they encode an empirical finding.
- No new public API without XML docs; no route strings outside `SonarRoutes`.