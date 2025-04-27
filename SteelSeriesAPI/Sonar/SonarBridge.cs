using SteelSeriesAPI.Sonar.Interfaces;
using SteelSeriesAPI.Sonar.Interfaces.Managers;
using SteelSeriesAPI.Sonar.Managers;

using System.Security.Principal;

namespace SteelSeriesAPI.Sonar;

/// <summary>
/// The Sonar object, to control Sonar<br/>Allow you to listen for event, get or set volumes, muted states, ...
/// </summary>
public class SonarBridge : ISonarBridge
{
    public bool IsRunning => SonarRetriever.Instance is { IsEnabled: true, IsReady: true, IsRunning: true };
    
    private ISonarSocket _sonarSocket;

    public readonly IModeManager Mode;
    public readonly IVolumeSettingsManager VolumeSettings;
    public readonly IChatMixManager ChatMix;
    public readonly IConfigurationManager Configurations;
    public readonly PlaybackDeviceManager PlaybackDevices;
    public readonly IRoutedProcessManager RoutedProcesses;
    public readonly IMixManager Mix;
    public readonly AudienceMonitoringManager AudienceMonitoring;
    public readonly EventManager Events;

    public SonarBridge()
    {
        Mode = new ModeManager();
        VolumeSettings = new VolumeSettingsManager();
        ChatMix = new ChatMixManager();
        Configurations = new ConfigurationManager();
        PlaybackDevices = new PlaybackDeviceManager();
        RoutedProcesses = new RoutedProcessManager();
        Mix = new MixManager();
        AudienceMonitoring = new AudienceMonitoringManager();
        Events = new EventManager();
    }

    #region Listener
    
    public bool StartListener()
    {
        if (!IsRunAsAdmin())
        {
            throw new ApplicationException("Listener requires Administrator rights to be used");
        }
        
        if (_sonarSocket != null && _sonarSocket.IsConnected)
        {
            throw new Exception("Listener already started");
        }
        
        _sonarSocket = new SonarSocket(Events);
        
        var connected = _sonarSocket.Connect();
        if (!connected)
        {
            return false;
        }

        var listening = _sonarSocket.Listen();
        if (!listening)
        {
            return false;
        }

        return true;
    }

    public void StopListener()
    {
        _sonarSocket.CloseSocket();
    }
    
    private bool IsRunAsAdmin()
    {
        WindowsIdentity id = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new WindowsPrincipal(id);

        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
    
    #endregion

    /// <summary>
    /// Wait until SteelSeries GG is started and running before running your code below
    /// </summary>
    public void WaitUntilSteelSeriesStarted()
    {
        SteelSeriesRetriever.Instance.WaitUntilSteelSeriesStarted();
    }

    /// <summary>
    /// Wait until Sonar is started and running before running your code below
    /// </summary>
    public void WaitUntilSonarStarted()
    {
        SonarRetriever.Instance.WaitUntilAppStarted();
    }
}