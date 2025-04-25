using System.Diagnostics;
using System.Text.Json;
using SteelSeriesAPI.Interfaces;

namespace SteelSeriesAPI.Sonar;

public class SonarRetriever : IAppRetriever
{
    private static readonly Lazy<SonarRetriever> _instance = new(() => new SonarRetriever());
    public static SonarRetriever Instance => _instance.Value;
    
    public string Name => "sonar";
    
    public bool IsEnabled => GetMetaDatas()[0];
    public bool IsReady => GetMetaDatas()[1];
    public bool IsRunning => GetMetaDatas()[2];
    public bool ShouldAutoStart => GetMetaDatas()[3];
    public bool IsWindowsSupported => GetMetaDatas()[4];
    public bool IsMacSupported => GetMetaDatas()[5];
    public bool ToggleViaSettings => GetMetaDatas()[6];
    public bool IsBrowserViewSupported => GetMetaDatas()[7];

    private readonly HttpClient _httpClient;
    
    /// <summary>
    /// Get information about Sonar such as meta datas
    /// </summary>
    public SonarRetriever()
    {
        HttpClientHandler clientHandler = new HttpClientHandler();
        clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
        _httpClient = new(clientHandler);
    }

    public bool[] GetMetaDatas()
    {
        if (!SteelSeriesRetriever.Instance.Running)
        {
            throw new Exception("SteelSeries Sonar is not running.");
        }

        try
        {
            JsonDocument subApps = JsonDocument.Parse(_httpClient.GetStringAsync("https://" + SteelSeriesRetriever.Instance.GetggEncryptedAddress() + "/subApps").Result);
            JsonElement appElement = subApps.RootElement.GetProperty("subApps").GetProperty(Name);

            bool isEnabled = appElement.GetProperty("isEnabled").GetBoolean();
            bool isReady = appElement.GetProperty("isReady").GetBoolean();
            bool isRunning = appElement.GetProperty("isRunning").GetBoolean();
            bool shouldAutoStart = appElement.GetProperty("shouldAutoStart").GetBoolean();
            bool isWindowsSupported = appElement.GetProperty("isWindowsSupported").GetBoolean();
            bool isMacSupported = appElement.GetProperty("isMacSupported").GetBoolean();
            bool toggleViaSettings = appElement.GetProperty("toggleViaSettings").GetBoolean();
            bool isBrowserViewSupported = appElement.GetProperty("isBrowserViewSupported").GetBoolean();

            return new bool[8] { isEnabled, isReady, isRunning, shouldAutoStart, isWindowsSupported,
                isMacSupported, toggleViaSettings, isBrowserViewSupported };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    /// <summary>
    /// Get the ip address of the Soanr rest server
    /// </summary>
    /// <returns>The ip address op the Sonar rest server</returns>
    /// <exception cref="Exception"></exception>
    public string WebServerAddress()
    {
        if (!IsEnabled || !IsReady || !IsRunning)
        {
            throw new Exception("SteelSeries Sonar not running");
        }
        
        JsonDocument subApps = JsonDocument.Parse(_httpClient.GetStringAsync("https://" + SteelSeriesRetriever.Instance.GetggEncryptedAddress() + "/subApps").Result);
        JsonElement appElement = subApps.RootElement.GetProperty("subApps").GetProperty(Name);

        return appElement.GetProperty("metadata").GetProperty("webServerAddress") + "/";
    }
    
    /// <summary>
    /// Wait until Sonar is started and running before running your code below
    /// </summary>
    public void WaitUntilAppStarted()
    {
        if (!SteelSeriesRetriever.Instance.Running)
        {
            SteelSeriesRetriever.Instance.WaitUntilSteelSeriesStarted();
        }
        
        if (!IsEnabled || !IsReady || !IsRunning)
        {
            Console.WriteLine("Waiting for Sonar to start");
            while (!IsEnabled || !IsReady || !IsRunning)
            {
                Thread.Sleep(500);
            }
            Console.WriteLine("Sonar started, continuing");
        }
    }
}