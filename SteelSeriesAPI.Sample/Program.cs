using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SteelSeriesAPI.Sonar;
using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sample;

internal static class Program
{
    private static async Task Main()
    {
        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(LogLevel.Debug));
        var logger = loggerFactory.CreateLogger("Sample");

        using var sonar = new SonarClient(logger);

        // --- 1. ModeManager validation: read, switch, time the confirmation, switch back ---
        var initialMode = await sonar.Mode.GetAsync();
        Console.WriteLine($"Current mode: {initialMode}");

        var target = initialMode == Mode.Classic ? Mode.Streamer : Mode.Classic;
        var sw = Stopwatch.StartNew();
        await sonar.Mode.SetAsync(target);
        sw.Stop();
        Console.WriteLine($"Switched to {target}, confirmed in {sw.ElapsedMilliseconds} ms");

        sw.Restart();
        await sonar.Mode.SetAsync(initialMode);
        sw.Stop();
        Console.WriteLine($"Switched back to {initialMode}, confirmed in {sw.ElapsedMilliseconds} ms");

        // --- 2. Route exploration sweep: V1 routes, do they still exist and what do they return? ---
        string[] candidateRoutes =
        [
            "chatMix",
            "configs",
            "audioDevices",
            "classicRedirections",
            "streamRedirections"
        ];

        foreach (string route in candidateRoutes)
        {
            Console.WriteLine($"\n=== GET {route} ===");
            try
            {
                using var doc = await sonar.GetRawAsync(route);
                Console.WriteLine(doc.RootElement);
            }
            catch (Exception e)
            {
                Console.WriteLine($"FAILED: {e.GetType().Name} - {e.Message}");
            }
        }
    }
}