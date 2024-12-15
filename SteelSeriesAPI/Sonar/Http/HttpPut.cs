using SteelSeriesAPI.Interfaces;

namespace SteelSeriesAPI.Sonar.Http;

public class HttpPut
{
    private readonly HttpClient _httpClient;
    private readonly IAppRetriever _sonarRetriever;

    public HttpPut(string targetHttp)
    {
        _sonarRetriever = SonarRetriever.Instance;
        
        HttpClientHandler clientHandler = new HttpClientHandler();
        clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
        _httpClient = new(clientHandler);
        
        Put(targetHttp);
    }

    private void Put(string targetHttp)
    {
        if (_sonarRetriever is { IsEnabled: false, IsReady: false, IsRunning: false })
        {
            Console.WriteLine("Sonar is not running.");
            return;
        }

        try
        {
            HttpResponseMessage httpResponseMessage = _httpClient
                .PutAsync(_sonarRetriever.WebServerAddress() + targetHttp, null)
                .GetAwaiter().GetResult();
            httpResponseMessage.EnsureSuccessStatusCode();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine("Sonar may not be running.");
            throw;
        }
    }
}