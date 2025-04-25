using System.Text.Json;
using SteelSeriesAPI.Interfaces;

namespace SteelSeriesAPI.Sonar.Http;

public class HttpFetcher
{
    private readonly HttpClient _httpClient;
    private readonly IAppRetriever _sonarRetriever;
    
    public HttpFetcher()
    {
        _sonarRetriever = SonarRetriever.Instance;
        
        HttpClientHandler clientHandler = new HttpClientHandler();
        clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
        _httpClient = new(clientHandler);
    }

    public JsonDocument Provide(string targetHttp)
    {
        JsonDocument response = JsonDocument.Parse(_httpClient.GetStringAsync(_sonarRetriever.WebServerAddress() + targetHttp).Result);
        return response;
    }

    public void Put(string targetHttp)
    {
        HttpResponseMessage httpResponseMessage = _httpClient
            .PutAsync(_sonarRetriever.WebServerAddress() + targetHttp, null)
            .GetAwaiter().GetResult();
        httpResponseMessage.EnsureSuccessStatusCode();
    }
}