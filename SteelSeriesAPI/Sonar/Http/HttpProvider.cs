using System.Text.Json;
using SteelSeriesAPI.Interfaces;

namespace SteelSeriesAPI.Sonar.Http;

public class HttpProvider
{
    private readonly HttpClient _httpClient;
    private readonly IAppRetriever _sonarRetriever;
    
    private string _targetHttp;
    
    public HttpProvider(string targetHttp)
    {
        _targetHttp = targetHttp;
        _sonarRetriever = new SonarRetriever();
        
        HttpClientHandler clientHandler = new HttpClientHandler();
        clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
        _httpClient = new(clientHandler);
    }

    public JsonDocument Provide()
    {
        JsonDocument response = JsonDocument.Parse(_httpClient.GetStringAsync(_sonarRetriever.WebServerAddress() + _targetHttp).Result);
        return response;
    }
}