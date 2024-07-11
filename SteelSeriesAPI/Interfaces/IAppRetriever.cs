namespace SteelSeriesAPI.Interfaces;

public interface IAppRetriever
{
    string Name { get; }
    bool IsEnabled { get; }
    bool IsReady { get; }
    bool IsRunning { get; }
    bool ShouldAutoStart { get; }
    bool IsWindowsSupported { get; }
    bool IsMacSupported { get; }
    bool ToggleViaSettings { get; }
    bool IsBrowserViewSupported { get; }

    string WebServerAddress();

    void WaitUntilAppStarted();
}