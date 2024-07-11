namespace SteelSeriesAPI.Interfaces;

public interface ISteelSeriesRetriever
{
    bool Running { get; }

    string GetggEncryptedAddress();

    void WaitUntilSteelSeriesStarted();
}