using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using SteelSeriesAPI.Core;
using SteelSeriesAPI.Sonar;
using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Explorer;

/// <summary>
/// Interactive exploration and verification tool for the Sonar API. Not shipped with the library.
/// Run without arguments for the REPL, or with one command for scripting (e.g. `dotnet run -- check`).
/// </summary>
internal static class Program
{
    private static readonly string[] KnownGetRoutes =
    [
        "mode",
        "volumeSettings/classic/",
        "volumeSettings/streamer/",
        "v1/chatMix",
        "audioDevices",
        "classicRedirections",
        "streamRedirections",
        "streamRedirections/isStreamMonitoringEnabled",
        "AudioDeviceRouting",
        "configs"
    ];

    private static async Task Main(string[] args)
    {
        using var sonar = new SonarClient();

        if (args.Length > 0)
        {
            // Non-interactive mode: run one command and exit.
            await ExecuteAsync(sonar, args);
            return;
        }

        Console.WriteLine($"Sonar server: {await sonar.GetServerAddressAsync()}");
        Console.WriteLine("Commands: get <route> | put <route> | probe <r1> <r2>... | dump | check [update] | verify | ws [path] [message] | quit");

        while (true)
        {
            Console.Write("\n> ");
            string[] input = (Console.ReadLine() ?? "").Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (input.Length == 0) continue;
            if (input[0].ToLowerInvariant() is "quit" or "q") return;

            await ExecuteAsync(sonar, input);
        }
    }

    private static async Task ExecuteAsync(SonarClient sonar, string[] input)
    {
        try
        {
            switch (input[0].ToLowerInvariant())
            {
                case "get" when input.Length == 2:
                    using (var doc = await sonar.GetRawAsync(input[1]))
                        Console.WriteLine(Pretty(doc.RootElement));
                    break;

                case "put" when input.Length == 2:
                    await sonar.PutRawAsync(input[1]);
                    Console.WriteLine("OK");
                    break;

                case "probe" when input.Length > 1:
                    foreach (string route in input[1..])
                        Console.WriteLine($"  {route,-45} -> {await Probe(sonar, route)}");
                    break;

                case "dump":
                    await Dump(sonar);
                    break;

                case "check":
                    await Check(sonar, update: input.Length > 1 && input[1].Equals("update", StringComparison.OrdinalIgnoreCase));
                    break;

                case "verify":
                    await Verify(sonar);
                    break;

                case "ws":
                    await ListenWebSocketAsync(
                        sonar,
                        input.Length >= 2 ? input[1] : "/",
                        input.Length >= 3 ? string.Join(' ', input[2..]) : null);
                    break;

                default:
                    Console.WriteLine("Unknown command.");
                    break;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"{e.GetType().Name}: {e.Message}");
        }
    }

    // ----------------------------------------------------------------
    // check: structural regression detection across GG updates
    // ----------------------------------------------------------------

    /// <summary>
    /// Compares the structure of every known GET route against committed reference shapes.
    /// Values are replaced by their types, so volatile content (GUIDs, volumes, dates,
    /// new presets) is invisible: only renamed/removed/retyped fields report a change.
    /// Runs in streamer mode (the superset: classic responses have empty "stream" sections)
    /// and restores the initial mode afterwards.
    /// Run 'check update' once to create or refresh the references.
    /// </summary>
    private static async Task Check(SonarClient sonar, bool update)
    {
        Directory.CreateDirectory(ReferenceDir);

        // Reference shapes are captured in streamer mode; checking in classic mode would
        // report false "STRUCTURE CHANGED" on the mode-dependent routes (empty stream sections).
        Mode initialMode = await sonar.Mode.GetAsync();
        if (initialMode != Mode.Streamer)
        {
            Console.WriteLine("Switching to streamer mode for the check (will restore afterwards)...");
            await sonar.Mode.SetAsync(Mode.Streamer);
        }

        int ok = 0, changed = 0, failed = 0;

        try
        {
            foreach (string route in KnownGetRoutes)
            {
                string file = Path.Combine(ReferenceDir, SafeName(route) + ".shape.json");
                try
                {
                    using var doc = await sonar.GetRawAsync(route);
                    string shape = PrettyShape(NormalizeShape(doc.RootElement));

                    if (update)
                    {
                        await File.WriteAllTextAsync(file, shape);
                        Console.WriteLine($"  {route,-45} UPDATED");
                        continue;
                    }

                    if (!File.Exists(file))
                    {
                        Console.WriteLine($"  {route,-45} NO REFERENCE (run 'check update' once)");
                        changed++;
                    }
                    else if (await File.ReadAllTextAsync(file) == shape)
                    {
                        Console.WriteLine($"  {route,-45} OK");
                        ok++;
                    }
                    else
                    {
                        string actualFile = file + ".actual";
                        await File.WriteAllTextAsync(actualFile, shape);
                        Console.WriteLine($"  {route,-45} STRUCTURE CHANGED -> diff {Path.GetFileName(file)} vs {Path.GetFileName(actualFile)}");
                        changed++;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"  {route,-45} FAILED: {e.GetType().Name}: {e.Message}");
                    failed++;
                }
            }
        }
        finally
        {
            if (initialMode != Mode.Streamer)
            {
                Console.WriteLine($"Restoring initial mode ({initialMode})...");
                await sonar.Mode.SetAsync(initialMode);
            }
        }

        Console.WriteLine(update
            ? $"\nReference shapes written to {ReferenceDir} - commit them."
            : $"\n{ok} OK, {changed} changed, {failed} failed." +
              (changed + failed == 0 ? " The API structure is compatible with this library." : ""));
    }

    /// <summary>
    /// Reduces a JSON payload to its structure: values become type names, object keys
    /// are sorted, and array elements are collapsed to their distinct shapes.
    /// The result is itself valid JSON, so it can be pretty-printed for diffing.
    /// </summary>
    internal static string NormalizeShape(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Object => "{" + string.Join(",", element.EnumerateObject()
            .OrderBy(p => p.Name, StringComparer.Ordinal)
            .Select(p => $"{JsonSerializer.Serialize(p.Name)}:{NormalizeShape(p.Value)}")) + "}",

        JsonValueKind.Array => "[" + string.Join(",", element.EnumerateArray()
            .Select(NormalizeShape)
            .Distinct()
            .OrderBy(s => s, StringComparer.Ordinal)) + "]",

        JsonValueKind.String => "\"string\"",
        JsonValueKind.Number => "\"number\"",
        JsonValueKind.True or JsonValueKind.False => "\"boolean\"",
        JsonValueKind.Null => "\"null\"",
        _ => "\"unknown\""
    };

    private static string PrettyShape(string shapeJson)
    {
        using var doc = JsonDocument.Parse(shapeJson);
        return Pretty(doc.RootElement);
    }

    // ----------------------------------------------------------------
    // verify: live write round-trips against the real API
    // ----------------------------------------------------------------

    /// <summary>Marks a verify step as skipped rather than failed.</summary>
    private sealed class SkipException(string message) : Exception(message);

    /// <summary>
    /// Exercises every write route with a reversible round-trip (set, read back, restore).
    /// Run after a GG update to confirm the write contracts still hold.
    /// </summary>
    private static async Task Verify(SonarClient sonar)
    {
        int pass = 0, fail = 0, skip = 0;

        async Task Step(string name, Func<Task> action)
        {
            try
            {
                await action();
                Console.WriteLine($"  PASS  {name}");
                pass++;
            }
            catch (SkipException e)
            {
                Console.WriteLine($"  SKIP  {name} ({e.Message})");
                skip++;
            }
            catch (Exception e)
            {
                Console.WriteLine($"  FAIL  {name}: {e.GetType().Name}: {e.Message}");
                fail++;
            }
        }

        Console.WriteLine("Running live write round-trips (state is restored after each step)...\n");

        Mode initialMode = await sonar.Mode.GetAsync();

        // ---- classic-mode steps ----
        await Step("Switch to Classic mode", () => sonar.Mode.SetAsync(Mode.Classic));

        await Step("Classic volume round-trip", async () =>
        {
            var before = await sonar.VolumeSettings.GetAsync(Channel.Game);
            double target = Math.Abs(before.Volume - 0.42) < 0.01 ? 0.50 : 0.42;

            await sonar.VolumeSettings.SetVolumeAsync(Channel.Game, target);
            var after = await sonar.VolumeSettings.GetAsync(Channel.Game);
            if (Math.Abs(after.Volume - target) > 0.005)
                throw new Exception($"read back {after.Volume}, expected {target}");

            await sonar.VolumeSettings.SetVolumeAsync(Channel.Game, before.Volume);
        });

        await Step("Classic mute round-trip", async () =>
        {
            var before = await sonar.VolumeSettings.GetAsync(Channel.Game);

            await sonar.VolumeSettings.SetMuteAsync(Channel.Game, !before.Muted);
            var after = await sonar.VolumeSettings.GetAsync(Channel.Game);
            if (after.Muted == before.Muted)
                throw new Exception("mute state did not change");

            await sonar.VolumeSettings.SetMuteAsync(Channel.Game, before.Muted);
        });

        await Step("ChatMix round-trip", async () =>
        {
            var before = await sonar.ChatMix.GetAsync();
            if (!string.Equals(before.State, "enabled", StringComparison.OrdinalIgnoreCase))
                throw new SkipException($"chat mix state is '{before.State}'");

            double target = Math.Abs(before.Balance) < 0.01 ? 0.20 : 0.00;
            await sonar.ChatMix.SetAsync(target);
            var after = await sonar.ChatMix.GetAsync();
            if (Math.Abs(after.Balance - target) > 0.005)
                throw new Exception($"read back {after.Balance}, expected {target}");

            await sonar.ChatMix.SetAsync(before.Balance);
        });

        await Step("Config re-select", async () =>
        {
            // Selecting the already-selected config exercises the route without changing anything.
            var selected = await sonar.Configs.GetSelectedAsync(Channel.Game)
                ?? throw new SkipException("no selected config reported for Game");
            await sonar.Configs.SelectAsync(selected.Id);
        });

        // ---- streamer-mode steps ----
        await Step("Switch to Streamer mode", () => sonar.Mode.SetAsync(Mode.Streamer));

        await Step("Streamer volume round-trip", async () =>
        {
            var before = await sonar.VolumeSettings.GetAsync(Channel.Game, Mix.Personal);
            double target = Math.Abs(before.Volume - 0.42) < 0.01 ? 0.50 : 0.42;

            await sonar.VolumeSettings.SetVolumeAsync(Channel.Game, Mix.Personal, target);
            var after = await sonar.VolumeSettings.GetAsync(Channel.Game, Mix.Personal);
            if (Math.Abs(after.Volume - target) > 0.005)
                throw new Exception($"read back {after.Volume}, expected {target}");

            await sonar.VolumeSettings.SetVolumeAsync(Channel.Game, Mix.Personal, before.Volume);
        });

        await Step("Stream monitoring round-trip", async () =>
        {
            bool before = await sonar.Redirections.GetStreamMonitoringEnabledAsync();

            await sonar.Redirections.SetStreamMonitoringEnabledAsync(!before);
            await Task.Delay(150); // give Sonar a beat before reading back
            if (await sonar.Redirections.GetStreamMonitoringEnabledAsync() == before)
                throw new Exception("monitoring state did not change");

            await sonar.Redirections.SetStreamMonitoringEnabledAsync(before);
        });

        await Step("Mix channel toggle round-trip", async () =>
        {
            var state = await sonar.Redirections.GetStreamRedirectionsAsync();
            var personal = state.Personal ?? throw new SkipException("personal mix absent from response");
            if (!personal.EnabledChannels.TryGetValue(Channel.Media, out bool before))
                throw new SkipException("Media channel absent from personal mix");

            await sonar.Redirections.SetMixChannelEnabledAsync(Mix.Personal, Channel.Media, !before);
            await Task.Delay(150);
            var after = await sonar.Redirections.GetStreamRedirectionsAsync();
            if (after.Personal?.EnabledChannels.GetValueOrDefault(Channel.Media) == before)
                throw new Exception("mix toggle did not change");

            await sonar.Redirections.SetMixChannelEnabledAsync(Mix.Personal, Channel.Media, before);
        });

        await Step("Streamer mic device round-trip", async () =>
        {
            var state = await sonar.Redirections.GetStreamRedirectionsAsync();
            var mic = state.Mic ?? throw new SkipException("mic redirection absent from response");

            var captures = (await sonar.Devices.GetAllAsync(AudioDataFlow.Capture))
                .Where(d => !d.IsSonarVirtual).ToList();
            var other = captures.FirstOrDefault(d => d.Id != mic.DeviceId)
                ?? throw new SkipException("only one capture device available");

            await sonar.Redirections.SetMicDeviceAsync(other.Id);
            await Task.Delay(150);
            var after = await sonar.Redirections.GetStreamRedirectionsAsync();
            if (after.Mic?.DeviceId != other.Id)
                throw new Exception($"read back {after.Mic?.DeviceId}, expected {other.Id}");

            await sonar.Redirections.SetMicDeviceAsync(mic.DeviceId);
        });

        await Step($"Restore initial mode ({initialMode})", () => sonar.Mode.SetAsync(initialMode));

        Console.WriteLine($"\n{pass} passed, {fail} failed, {skip} skipped." +
                          (fail == 0 ? " All write contracts hold." : " Investigate the failures before trusting the library."));
    }

    // ----------------------------------------------------------------
    // Existing commands
    // ----------------------------------------------------------------

    /// <summary>Tries a GET on a route and describes the outcome without throwing.</summary>
    private static async Task<string> Probe(SonarClient sonar, string route)
    {
        try
        {
            using var doc = await sonar.GetRawAsync(route);
            string preview = doc.RootElement.GetRawText();
            return $"200 OK ({preview[..Math.Min(60, preview.Length)]}...)";
        }
        catch (SonarWrongModeException) { return "500 WRONG MODE (route exists!)"; }
        catch (SonarRequestException e) { return $"{e.StatusCode}"; }
    }

    /// <summary>Snapshots every known GET route into dated JSON files, for diffing across GG updates.</summary>
    private static async Task Dump(SonarClient sonar)
    {
        string dir = Path.Combine(ProjectDir, "dumps", DateTime.Now.ToString("yyyy-MM-dd_HHmm"));
        Directory.CreateDirectory(dir);

        foreach (string route in KnownGetRoutes)
        {
            string file = Path.Combine(dir, SafeName(route) + ".json");
            try
            {
                using var doc = await sonar.GetRawAsync(route);
                await File.WriteAllTextAsync(file, Pretty(doc.RootElement));
                Console.WriteLine($"  {route,-45} -> {file}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"  {route,-45} -> FAILED: {e.Message}");
            }
        }
    }

    /// <summary>Connects to a WebSocket path on the Sonar server and prints every incoming message.</summary>
    private static async Task ListenWebSocketAsync(SonarClient sonar, string path, string? initialMessage)
    {
        Uri http = await sonar.GetServerAddressAsync();
        Uri wsUri = new UriBuilder(http) { Scheme = "ws", Path = path }.Uri;
        Console.WriteLine($"Connecting to {wsUri} ... (press Enter to stop)");

        using var ws = new ClientWebSocket();
        await ws.ConnectAsync(wsUri, CancellationToken.None);
        Console.WriteLine("Connected! Now interact with the Sonar UI (sliders, mute, mode...)");

        using var cts = new CancellationTokenSource();

        if (initialMessage is not null)
        {
            await ws.SendAsync(Encoding.UTF8.GetBytes(initialMessage),
                WebSocketMessageType.Text, endOfMessage: true, cts.Token);
            Console.WriteLine($"Sent: {initialMessage}");
        }

        _ = Task.Run(() => { Console.ReadLine(); cts.Cancel(); });

        var buffer = new byte[64 * 1024];
        using var message = new MemoryStream();

        try
        {
            while (ws.State == WebSocketState.Open)
            {
                var result = await ws.ReceiveAsync(buffer, cts.Token);
                if (result.MessageType == WebSocketMessageType.Close) break;

                message.Write(buffer, 0, result.Count);
                if (!result.EndOfMessage) continue;

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ({result.MessageType}) {Encoding.UTF8.GetString(message.ToArray())}");
                message.SetLength(0);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Stopped listening.");
        }
    }

    // ----------------------------------------------------------------
    // Helpers
    // ----------------------------------------------------------------

    /// <summary>The Explorer project directory, so dumps and references survive `dotnet clean`.</summary>
    private static string ProjectDir { get; } = FindProjectDir();

    private static string ReferenceDir => Path.Combine(ProjectDir, "reference-shapes");

    private static string FindProjectDir()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && dir.GetFiles("*.csproj").Length == 0)
            dir = dir.Parent;
        return dir?.FullName ?? Directory.GetCurrentDirectory();
    }

    private static string SafeName(string route) =>
        route.Trim('/').Replace('/', '_').Replace('?', '_');

    private static string Pretty(JsonElement element) =>
        JsonSerializer.Serialize(element, new JsonSerializerOptions { WriteIndented = true });
}