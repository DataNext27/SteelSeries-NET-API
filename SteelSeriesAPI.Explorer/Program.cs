using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using SteelSeriesAPI.Core;
using SteelSeriesAPI.Sonar;

namespace SteelSeriesAPI.Explorer;

/// <summary>Interactive exploration tool for the Sonar API. Not shipped with the library.</summary>
internal static class Program
{
    private static readonly string[] KnownGetRoutes =
    [
        "mode",
        "volumeSettings/classic/",
        "volumeSettings/streamer/",
        "audioDevices",
        "classicRedirections",
        "streamRedirections",
        "configs"
    ];

    private static async Task Main()
    {
        using var sonar = new SonarClient();
        Console.WriteLine($"Sonar server: {await sonar.GetServerAddressAsync()}");
        Console.WriteLine("Sonar API Explorer - commands: get <route> | put <route> | probe <r1> <r2> ... | dump | ws | quit");

        while (true)
        {
            Console.Write("\n> ");
            string[] input = (Console.ReadLine() ?? "").Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (input.Length == 0) continue;

            try
            {
                switch (input[0].ToLowerInvariant())
                {
                    case "quit" or "q":
                        return;

                    case "get" when input.Length == 2:
                        using (var doc = await sonar.GetRawAsync(input[1]))
                            Console.WriteLine(Pretty(doc));
                        break;

                    case "put" when input.Length == 2:
                        await sonar.PutRawAsync(input[1]);
                        Console.WriteLine("OK");
                        break;

                    case "probe" when input.Length > 1:
                        foreach (string route in input[1..])
                            Console.WriteLine($"  {route,-40} -> {await Probe(sonar, route)}");
                        break;

                    case "dump":
                        await Dump(sonar);
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
    }

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
        string dir = Path.Combine("dumps", DateTime.Now.ToString("yyyy-MM-dd_HHmm"));
        Directory.CreateDirectory(dir);

        foreach (string route in KnownGetRoutes)
        {
            string file = Path.Combine(dir, route.Trim('/').Replace('/', '_') + ".json");
            try
            {
                using var doc = await sonar.GetRawAsync(route);
                await File.WriteAllTextAsync(file, Pretty(doc));
                Console.WriteLine($"  {route,-30} -> {file}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"  {route,-30} -> FAILED: {e.Message}");
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
        var message = new MemoryStream();

        try
        {
            while (ws.State == WebSocketState.Open)
            {
                var result = await ws.ReceiveAsync(buffer, cts.Token);
                if (result.MessageType == WebSocketMessageType.Close) break;

                // A logical message may span several frames: accumulate until EndOfMessage
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

    private static string Pretty(JsonDocument doc) =>
        JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
}