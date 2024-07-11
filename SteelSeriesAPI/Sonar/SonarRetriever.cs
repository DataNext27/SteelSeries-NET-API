using System.Diagnostics;
using System.Text.Json;
using SteelSeriesAPI.Interfaces;

namespace SteelSeriesAPI.Sonar;

public class SonarRetriever : IAppRetriever
{
    public string Name => "sonar";
    
    public bool IsEnabled => GetMetaDatas()[0];
    public bool IsReady => GetMetaDatas()[1];
    public bool IsRunning => GetMetaDatas()[2];
    public bool ShouldAutoStart => GetMetaDatas()[3];
    public bool IsWindowsSupported => GetMetaDatas()[4];
    public bool IsMacSupported => GetMetaDatas()[5];
    public bool ToggleViaSettings => GetMetaDatas()[6];
    public bool IsBrowserViewSupported => GetMetaDatas()[7];

    private readonly ISteelSeriesRetriever _ssRetriever;
    private readonly string _ggEncryptedAddress;
    private readonly HttpClient _httpClient;
    public SonarRetriever()
    {
        _ssRetriever = new SteelSeriesRetriever();
        _ssRetriever.WaitUntilSteelSeriesStarted();
        _ggEncryptedAddress = _ssRetriever.GetggEncryptedAddress();
        
        HttpClientHandler clientHandler = new HttpClientHandler();
        clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
        _httpClient = new(clientHandler);
    }

    public bool[] GetMetaDatas()
    {
        if (!_ssRetriever.Running)
        {
            throw new Exception("SteelSeries not running");
        }

        try
        {
            JsonDocument subApps = JsonDocument.Parse(_httpClient.GetStringAsync("https://" + _ggEncryptedAddress + "/subApps").Result);
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
    
    public string WebServerAddress()
    {
        if (!IsEnabled || !IsReady || !IsRunning)
        {
            throw new Exception("SteelSeries Sonar not running");
        }
        
        JsonDocument subApps = JsonDocument.Parse(_httpClient.GetStringAsync("https://" + _ggEncryptedAddress + "/subApps").Result);
        JsonElement appElement = subApps.RootElement.GetProperty("subApps").GetProperty(Name);

        return appElement.GetProperty("metadata").GetProperty("webServerAddress") + "/";
    }
    
    /// <summary>
    /// Wait until Sonar is started and running before running your code below
    /// </summary>
    public void WaitUntilAppStarted()
    {
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