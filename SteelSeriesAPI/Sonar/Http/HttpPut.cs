using SteelSeriesAPI.Interfaces;

namespace SteelSeriesAPI.Sonar.Http;

public class HttpPut
{
    private readonly HttpClient _httpClient;
    private readonly IAppRetriever _sonarRetriever;
    
    private string _targetHttp;
    
    public HttpPut(string targetHttp)
    {
        _targetHttp = targetHttp;
        _sonarRetriever = SonarRetriever.Instance;
        
        HttpClientHandler clientHandler = new HttpClientHandler();
        clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
        _httpClient = new(clientHandler);
        
        Put(_targetHttp);
    }

    private void Put(string targetHttp)
    {
        HttpResponseMessage? httpResponseMessage = _httpClient.PutAsync(_sonarRetriever.WebServerAddress() + targetHttp, null)
            .GetAwaiter().GetResult();
        httpResponseMessage.EnsureSuccessStatusCode();
    }
}