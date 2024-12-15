using System.Text.Json;
using SteelSeriesAPI.Interfaces;

namespace SteelSeriesAPI.Sonar.Http;

public class HttpProvider
{
    private readonly HttpClient _httpClient;
    private readonly IAppRetriever _sonarRetriever;
    
    private readonly string _targetHttp;
    
    public HttpProvider(string targetHttp)
    {
        _targetHttp = targetHttp;
        _sonarRetriever = SonarRetriever.Instance;
        
        HttpClientHandler clientHandler = new HttpClientHandler();
        clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
        _httpClient = new(clientHandler);
    }

    public JsonDocument Provide()
    {
        if (_sonarRetriever is { IsEnabled: false, IsReady: false, IsRunning: false })
        {
            Console.WriteLine("Sonar is not running.");
            return null;
        }

        try
        {
            JsonDocument response = JsonDocument.Parse(_httpClient.GetStringAsync(_sonarRetriever.WebServerAddress() + _targetHttp).Result);
            return response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine("Sonar may not be running.");
            throw;
        }
    }
}