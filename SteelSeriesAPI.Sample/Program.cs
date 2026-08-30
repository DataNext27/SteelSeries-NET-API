using Microsoft.Extensions.Logging;
using SteelSeriesAPI.Core;
using SteelSeriesAPI.Sonar;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sample;

/// <summary>
/// Demo and manual test bench for the SteelSeries-NET-API library.
/// Reads the full current Sonar state, then listens to all events until Enter is pressed.
/// </summary>
internal static class Program
{
    private static async Task Main()
    {
        // Set to LogLevel.Debug to see discovery, reconnections and refresh internals
        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(LogLevel.Information));
        var logger = loggerFactory.CreateLogger("Sample");

        using var sonar = new SonarClient(logger);

        try
        {
            await PrintCurrentStateAsync(sonar);
        }
        catch (SteelSeriesException e)
        {
            Console.WriteLine($"Could not read Sonar state: {e.Message}");
            Console.WriteLine("Make sure SteelSeries GG is running, then restart the sample.");
            return;
        }

        SubscribeToEvents(sonar);

        sonar.Events.PollingInterval = TimeSpan.FromMilliseconds(500);
        sonar.Events.Start();

        Console.WriteLine();
        Console.WriteLine("=== Listening to Sonar events - interact with the Sonar UI, press Enter to stop ===");
        Console.WriteLine();
        Console.ReadLine();

        await sonar.Events.StopAsync();
        Console.WriteLine("Stopped. Bye!");
    }

    /// <summary>Reads and prints the current Sonar state using every typed manager.</summary>
    private static async Task PrintCurrentStateAsync(SonarClient sonar)
    {
        Console.WriteLine("=== Current Sonar state ===");

        // --- Mode ---
        Mode mode = await sonar.Mode.GetAsync();
        Console.WriteLine($"Mode: {mode}");

        // --- Audio devices (also used to resolve ids to names below) ---
        var devices = await sonar.Devices.GetAllAsync();
        var deviceNames = devices.ToDictionary(d => d.Id, d => d.Name);
        string NameOf(string deviceId) => deviceNames.GetValueOrDefault(deviceId, deviceId);

        // --- Volumes (query what is reliable in the current mode) ---
        Channel[] channels = [Channel.Master, Channel.Game, Channel.Chat, Channel.Media, Channel.Aux, Channel.Mic];

        if (mode == Mode.Classic)
        {
            foreach (Channel channel in channels)
            {
                var setting = await sonar.VolumeSettings.GetAsync(channel);
                Console.WriteLine($"  {channel,-6} volume: {setting.Volume,6:P0}   muted: {setting.Muted}");
            }
        }
        else
        {
            foreach (Channel channel in channels)
            {
                var personal = await sonar.VolumeSettings.GetAsync(channel, Mix.Personal);
                var stream = await sonar.VolumeSettings.GetAsync(channel, Mix.Stream);
                Console.WriteLine(
                    $"  {channel,-6} personal: {personal.Volume,6:P0} (muted: {personal.Muted})   " +
                    $"stream: {stream.Volume,6:P0} (muted: {stream.Muted})");
            }
        }

        // --- Chat mix ---
        var chatMix = await sonar.ChatMix.GetAsync();
        Console.WriteLine($"ChatMix: balance {chatMix.Balance:+0.00;-0.00;0.00} (state: {chatMix.State})");

        // --- Selected configs ---
        var selected = await sonar.Configs.GetSelectedAsync();
        Console.WriteLine("Selected configs:");
        foreach ((Channel channel, var config) in selected.OrderBy(p => p.Key))
            Console.WriteLine($"  {channel,-6} -> {config.Name}{(config.IsPreset ? " (preset)" : "")}");

        // --- Classic redirections ---
        var classicRedirections = await sonar.Redirections.GetClassicRedirectionsAsync();
        Console.WriteLine("Classic redirections:");
        foreach (var redirection in classicRedirections)
            Console.WriteLine($"  {redirection.Channel,-6} -> {NameOf(redirection.DeviceId)} (running: {redirection.IsRunning})");

        // --- Streamer-mode redirections (meaningful values in streamer mode only) ---
        if (mode == Mode.Streamer)
        {
            var streamRedirections = await sonar.Redirections.GetStreamRedirectionsAsync();
            Console.WriteLine("Stream redirections:");
            PrintMix(streamRedirections.Personal);
            PrintMix(streamRedirections.Stream);
            if (streamRedirections.Mic is { } mic)
                Console.WriteLine($"  Mic passthrough -> {NameOf(mic.DeviceId)} (running: {mic.IsRunning})");

            bool monitoring = await sonar.Redirections.GetStreamMonitoringEnabledAsync();
            Console.WriteLine($"Stream monitoring (hear the audience mix): {monitoring}");

            void PrintMix(MixRedirection? mix)
            {
                if (mix is null) return;
                string enabledChannels = string.Join(", ",
                    mix.EnabledChannels.Where(p => p.Value).Select(p => p.Key));
                Console.WriteLine($"  {mix.Mix,-8} mix -> {NameOf(mix.DeviceId)} (running: {mix.IsRunning}, enabled: [{enabledChannels}])");
            }
        }

        // --- App routing: which applications play on which channel ---
        var routings = await sonar.AppRouting.GetRoutingsAsync();
        Console.WriteLine("App routing (active sessions):");
        bool anySession = false;
        foreach (var routing in routings.Where(r => r.Channel is not null && r.DataFlow == AudioDataFlow.Render))
        {
            foreach (var session in routing.Sessions.Where(s => !s.IsSystemSound && s.IsActive))
            {
                Console.WriteLine($"  {routing.Channel,-6} -> {session.DisplayName} (pid {session.ProcessId})");
                anySession = true;
            }
        }
        if (!anySession)
            Console.WriteLine("  (no application is currently playing audio)");
    }

    /// <summary>Subscribes to every event the library exposes, printing each occurrence.</summary>
    private static void SubscribeToEvents(SonarClient sonar)
    {
        // --- Connection lifecycle (from the WebSocket loop) ---
        sonar.Events.Connected += (_, _) =>
            Console.WriteLine(">>> Connected to Sonar event stream");

        sonar.Events.Disconnected += (_, _) =>
            Console.WriteLine(">>> Disconnected from Sonar (GG closed? will keep retrying)");

        // --- Granular, data-carrying events (most consumers should use these) ---
        sonar.Events.VolumeChanged += (_, e) =>
        {
            string mix = e.Mix?.ToString() ?? "Classic";
            if (e.MuteToggled)
                Console.WriteLine($"[Volume] {e.Channel} ({mix}) is now {(e.IsMuted ? "MUTED" : "unmuted")}");
            else
                Console.WriteLine($"[Volume] {e.Channel} ({mix}): {e.PreviousVolume:P0} -> {e.NewVolume:P0}");
        };

        sonar.Events.ModeChanged += (_, e) =>
            Console.WriteLine($"[Mode] {e.PreviousMode} -> {e.NewMode}");

        sonar.Events.ChatMixChanged += (_, e) =>
            Console.WriteLine($"[ChatMix] balance {e.Balance:+0.00;-0.00;0.00} (state: {e.State})");

        sonar.Events.ClassicDeviceChanged += (_, e) =>
            Console.WriteLine($"[Redirections] {e.Channel} routed to {e.NewDeviceId}");

        sonar.Events.MixDeviceChanged += (_, e) =>
            Console.WriteLine($"[Redirections] {e.Mix} mix routed to {e.NewDeviceId}");
        
        sonar.Events.MicDeviceChanged += (_, e) =>
            Console.WriteLine($"[Redirections] Mic passthrough routed to {e.NewDeviceId}");

        sonar.Events.AudioDeviceStatusChanged += (_, e) =>
            Console.WriteLine($"[Devices] {e.Name} ({e.DataFlow}) is now {e.State}");

        sonar.Events.MixChannelToggled += (_, e) =>
            Console.WriteLine($"[Redirections] {e.Channel} on {e.Mix} mix: {(e.IsEnabled ? "enabled" : "disabled")}");

        sonar.Events.StreamMonitoringChanged += (_, e) =>
            Console.WriteLine($"[Monitoring] {(e.IsEnabled ? "hearing what the audience hears" : "back to personal mix")}");

        sonar.Events.ConfigSelectionChanged += (_, e) =>
            Console.WriteLine($"[Config] {e.Channel}: {e.PreviousConfig?.Name ?? "?"} -> {e.NewConfigName}");

        sonar.Events.AudioSessionOpened += (_, e) =>
        {
            var app = e.Sessions.FirstOrDefault(s => !s.IsSystemSound);
            if (app is not null)
                Console.WriteLine($"[Session] {app.DisplayName} (pid {app.ProcessId}) opened on {e.Channel?.ToString() ?? e.Role}");
        };

        sonar.Events.AudioSessionClosed += (_, e) =>
        {
            var app = e.Sessions.FirstOrDefault(s => !s.IsSystemSound);
            if (app is not null)
                Console.WriteLine($"[Session] {app.DisplayName} closed on {e.Channel?.ToString() ?? e.Role}");
        };

        // --- Raw invalidation signals (diagnostics; prefer the granular events above) ---
        sonar.Events.RedirectionsInvalidated += (_, _) =>
            Console.WriteLine("  (raw: redirections invalidated)");

        sonar.Events.ConfigsInvalidated += (_, _) =>
            Console.WriteLine("  (raw: configs invalidated)");

        sonar.Events.RoutingInvalidated += (_, _) =>
            Console.WriteLine("  (raw: app routing invalidated)");

        // --- Low-level / diagnostics ---
        sonar.Events.VolumeDataReceived += (_, e) =>
            Console.WriteLine($"[Snapshot] full volume state received ({e.Channels.Count} channels)");

        sonar.Events.UnknownEventReceived += (_, e) =>
            Console.WriteLine($"[Unknown] {e.EventName}");
    }
}