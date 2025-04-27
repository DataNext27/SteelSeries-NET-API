using SteelSeriesAPI.Sonar.Exceptions;
using SteelSeriesAPI.Interfaces;

using System.Text.Json;

namespace SteelSeriesAPI.Sonar.Http;

public class Fetcher
{
    private readonly HttpClient _httpClient;
    private readonly IAppRetriever _sonarRetriever;
    
    public Fetcher()
    {
        _sonarRetriever = SonarRetriever.Instance;
        
        HttpClientHandler clientHandler = new HttpClientHandler();
        clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
        _httpClient = new(clientHandler);
    }

    public JsonDocument Provide(string targetHttp)
    {
        if (_sonarRetriever is { IsEnabled: false, IsReady: false, IsRunning: false })
        {
            throw new SonarNotRunningException();
        }

        try
        {
            JsonDocument response = JsonDocument.Parse(_httpClient.GetStringAsync(_sonarRetriever.WebServerAddress() + targetHttp).Result);
            return response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine("Sonar may not be running.");
            throw;
        }
    }

    public void Put(string targetHttp)
    {
        if (_sonarRetriever is { IsEnabled: false, IsReady: false, IsRunning: false })
        {
            throw new SonarNotRunningException();
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