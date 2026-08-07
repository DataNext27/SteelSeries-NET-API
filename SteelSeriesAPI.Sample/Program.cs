using Microsoft.Extensions.Logging;
using SteelSeriesAPI.Core;
using SteelSeriesAPI.Sonar;
using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sample;

internal static class Program
{
    private static async Task Main()
    {
        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(LogLevel.Debug));
        var logger = loggerFactory.CreateLogger("Sample");

        var discovery = new ServerDiscovery(logger);
        using var client = new SonarHttpClient(discovery, logger);

        using var sonar = new SonarClient(logger);

        try
        {
            await sonar.VolumeSettings.SetVolumeAsync(Channel.Game, 0.5);
            Console.WriteLine("Classic set: OK (hypothesis rejected!)");
        }
        catch (SonarRequestException e)
        {
            Console.WriteLine($"Classic set rejected: HTTP {e.StatusCode}, body: '{e.ResponseBody}'");
        }

// 2. Streamer-route write should succeed
        await sonar.VolumeSettings.SetVolumeAsync(Channel.Game, Mix.Personal, 0.42);
        var check = await sonar.VolumeSettings.GetAsync(Channel.Game, Mix.Personal);
        Console.WriteLine($"Streamer set check: {check}");

// 3. Raw dumps for the test fixtures
        using var classic = await client.GetAsync("volumeSettings/classic/", default);
        Console.WriteLine(classic.RootElement);
        using var streamer = await client.GetAsync("volumeSettings/streamer/", default);
        Console.WriteLine(streamer.RootElement);

// 4. Bonus: what does the mode route say? (route from your V1)
        using var mode = await client.GetAsync("mode/", default);
        Console.WriteLine($"Mode: {mode.RootElement}");
    }
}