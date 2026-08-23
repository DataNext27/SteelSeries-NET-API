using Microsoft.Extensions.Logging;
using SteelSeriesAPI.Core;
using SteelSeriesAPI.Sonar;
using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sample;

/// <summary>
/// Demo and manual test bench for the SteelSeries-NET-API library.
/// Reads the full current Sonar state, then listens to all events until Enter is pressed.
/// </summary>
internal static class Program
{
    private static async Task Main()
    {
        // Set to LogLevel.Debug to see discovery, reconnections and polling internals
        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(LogLevel.Debug));
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

    /// <summary>Reads and prints the current mode, volumes, and chat mix using the typed managers.</summary>
    private static async Task PrintCurrentStateAsync(SonarClient sonar)
    {
        Console.WriteLine("=== Current Sonar state ===");

        // --- Mode ---
        Mode mode = await sonar.Mode.GetAsync();
        Console.WriteLine($"Mode: {mode}");

        // --- Volumes (query the channels relevant to the current mode) ---
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
    }

    /// <summary>Subscribes to every event the library exposes, printing each occurrence.</summary>
    private static void SubscribeToEvents(SonarClient sonar)
    {
        // --- Connection lifecycle (from the WebSocket loop) ---
        sonar.Events.Connected += (_, _) =>
            Console.WriteLine(">>> Connected to Sonar event stream");

        sonar.Events.Disconnected += (_, _) =>
            Console.WriteLine(">>> Disconnected from Sonar (GG closed? will keep retrying)");

        // --- Granular changes (most consumers should use these) ---
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

        // --- Invalidations (Sonar says "something changed" without details) ---
        sonar.Events.RedirectionsInvalidated += (_, _) =>
            Console.WriteLine("[Invalidated] redirection invalidation received from Sonar");
        sonar.Events.ClassicDeviceChanged += (_, e) =>
            Console.WriteLine($"[Redirections] {e.Channel} routed to {e.NewDeviceId}");
        sonar.Events.MixDeviceChanged += (_, e) =>
            Console.WriteLine($"[Redirections] {e.Mix} mix routed to {e.NewDeviceId}");
        sonar.Events.MixChannelToggled += (_, e) =>
            Console.WriteLine($"[Redirections] {e.Channel} on {e.Mix} mix: {(e.IsEnabled ? "enabled" : "disabled")}");
        sonar.Events.StreamMonitoringChanged += (_, e) =>
            Console.WriteLine($"[Monitoring] {(e.IsEnabled ? "hearing what the audience hears" : "back to personal mix")}");

        sonar.Events.SelectedConfigChanged += (_, _) =>
            Console.WriteLine("[Config] selected config changed");

        // --- Low-level / diagnostics ---
        sonar.Events.VolumeDataReceived += (_, e) =>
            Console.WriteLine($"[Snapshot] full volume state received ({e.Channels.Count} channels)");

        sonar.Events.UnknownEventReceived += (_, e) =>
            Console.WriteLine($"[Unknown] {e.EventName}");
    }
}
