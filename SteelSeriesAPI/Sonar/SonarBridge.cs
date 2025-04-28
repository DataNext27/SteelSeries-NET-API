using SteelSeriesAPI.Sonar.Interfaces;
using SteelSeriesAPI.Sonar.Interfaces.Managers;
using SteelSeriesAPI.Sonar.Managers;
using SteelSeriesAPI.Sonar.Enums;

using System.Security.Principal;
using System.Runtime.Versioning;

namespace SteelSeriesAPI.Sonar;

/// <summary>
/// The Sonar object, to control Sonar<br/>Allow you to listen for event, get or set volumes, muted states, ...
/// </summary>
public class SonarBridge : ISonarBridge
{
    /// <summary>
    /// The running state of Sonar
    /// </summary>
    public bool IsRunning => SonarRetriever.Instance is { IsEnabled: true, IsReady: true, IsRunning: true };
    
    private ISonarSocket _sonarSocket;

    /// <summary>
    /// Manage the Sonar <see cref="Mode"/>
    /// </summary>
    public readonly IModeManager Mode;
    
    /// <summary>
    /// Manage the volumes and muted state of each <see cref="Channel"/>
    /// </summary>
    public readonly IVolumeSettingsManager VolumeSettings;
    
    /// <summary>
    /// Manage the balance of ChatMix
    /// </summary>
    public readonly IChatMixManager ChatMix;
    
    /// <summary>
    /// Manage audio configurations for each <see cref="Channel"/>
    /// </summary>
    public readonly IConfigurationManager Configurations;
    
    /// <summary>
    /// Manage the playback device of each <see cref="Channel"/>
    /// </summary>
    public readonly IPlaybackDeviceManager PlaybackDevices;
    
    /// <summary>
    /// Manage routed audio processes
    /// </summary>
    public readonly IRoutedProcessManager RoutedProcesses;
    
    /// <summary>
    /// Manage the personal and stream mix for each channel
    /// </summary>
    public readonly IMixManager Mix;
    
    /// <summary>
    /// Manage the Audience Monitoring feature of the streamer mode
    /// </summary>
    public readonly IAudienceMonitoringManager AudienceMonitoring;
    
    /// <summary>
    /// Manage the different Sonar Events
    /// </summary>
    public readonly EventManager Events;

    /// <summary>
    /// The Sonar object, to control Sonar<br/>Allow you to listen for event, get or set volumes, muted states, ...
    /// </summary>
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
    
    /// <summary>
    /// Start listening to events happening on Sonar, such as changing volume...
    /// </summary>
    /// <returns>The state of the listener (false if it didn't start)</returns>
    [SupportedOSPlatform("windows")]
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

    /// <summary>
    /// Stop the listener
    /// </summary>
    [SupportedOSPlatform("windows")]
    public void StopListener()
    {
        _sonarSocket.CloseSocket();
    }
    
    [SupportedOSPlatform("windows")]
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