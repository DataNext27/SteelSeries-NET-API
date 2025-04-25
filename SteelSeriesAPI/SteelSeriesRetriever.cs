using System.Diagnostics;
using System.Text.Json;
using SteelSeriesAPI.Interfaces;

namespace SteelSeriesAPI;

public class SteelSeriesRetriever : ISteelSeriesRetriever
{
    private static readonly Lazy<SteelSeriesRetriever> _instance = new(() => new SteelSeriesRetriever());
    
    public static SteelSeriesRetriever Instance => _instance.Value;
    
    public bool Running => SteelSeriesProcessesChecker();
    
    private Process[] _steelSeriesProcesses;

    public SteelSeriesRetriever()
    {
        _steelSeriesProcesses = Process.GetProcessesByName("SteelSeriesSonar");
    }

    public string GetggEncryptedAddress()
    {
        if (!Running)
        {
            throw new Exception("SteelSeries is not started");
        }
        
        try
        {
            JsonDocument coreProps = JsonDocument.Parse(File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"SteelSeries\GG\coreProps.json")));
            string ggEncryptedAddress = coreProps.RootElement.GetProperty("ggEncryptedAddress").ToString();
            return ggEncryptedAddress;
        }
        catch (Exception e)
        {
            throw new Exception("Could not find coreProps.json\nIs SteelSeries installed?", e);
        }
    }

    public void WaitUntilSteelSeriesStarted()
    {
        if (!Running)
        {
            Console.WriteLine("Waiting for SteelSeries to start");
            while (!Running)
            {
                Thread.Sleep(500);
            }
            Console.WriteLine("SteelSeries started, continuing");
        }
    }

    private bool SteelSeriesProcessesChecker()
    {
        _steelSeriesProcesses = Process.GetProcessesByName("SteelSeriesSonar");
        return _steelSeriesProcesses.Length > 0;
    }
}