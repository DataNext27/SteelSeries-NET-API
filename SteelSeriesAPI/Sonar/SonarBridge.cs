using System.Security.Principal;
using SteelSeriesAPI.Interfaces;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Http;
using SteelSeriesAPI.Sonar.Interfaces;
using SteelSeriesAPI.Sonar.Managers;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar;

/// <summary>
/// The Sonar object, to control Sonar<br/>Allow you to listen for event, get or set volumes, muted states, ...
/// </summary>
public class SonarBridge : ISonarBridge
{
    public bool IsRunning => _sonarRetriever is { IsEnabled: true, IsReady: true, IsRunning: true };
    
    private readonly IAppRetriever _sonarRetriever;
    private readonly ISonarCommandHandler _sonarCommand;
    private readonly ISonarDataProvider _sonarProvider;
    private readonly ISonarSocket _sonarSocket;

    public readonly VolumeSettingsManager VolumeSettings;
    public readonly ConfigurationManager Configurations;
    public readonly PlaybackDeviceManager PlaybackDevices;
    public readonly EventManager Event;

    private string _sonarWebServerAddress;

    public SonarBridge()
    {
        _sonarRetriever = SonarRetriever.Instance;
        WaitUntilSonarStarted();
        _sonarWebServerAddress = _sonarRetriever.WebServerAddress();
        _sonarSocket = new SonarSocket(_sonarWebServerAddress, Event);
        _sonarCommand = new SonarHttpCommand(this);
        _sonarProvider = new SonarHttpProvider();
        VolumeSettings = new VolumeSettingsManager();
        Configurations = new ConfigurationManager();
        PlaybackDevices = new PlaybackDeviceManager();
        Event = new EventManager();
    }

    #region Listener
    
    public bool StartListener()
    {
        if (!IsRunAsAdmin())
        {
            throw new ApplicationException("Listener requires Administrator rights to be used");
        }
        
        if (_sonarSocket.IsConnected)
        {
            throw new Exception("Listener already started");
        }
        
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
    /// Wait until Sonar is started and running before running your code below
    /// </summary>
    public void WaitUntilSonarStarted()
    {
        _sonarRetriever.WaitUntilAppStarted();
    }

    #region Providers

    public Mode GetMode()
    {
        return _sonarProvider.GetMode();
    }
    
    public double GetChatMixBalance()
    {
        return _sonarProvider.GetChatMixBalance();
    }

    public bool GetChatMixState()
    {
        return _sonarProvider.GetChatMixState();
    }

    public bool GetRedirectionState(Channel channel, Mix mix)
    {
        return _sonarProvider.GetRedirectionState(channel, mix);
    }

    public bool GetAudienceMonitoringState()
    {
        return _sonarProvider.GetAudienceMonitoringState();
    }

    public IEnumerable<RoutedProcess> GetRoutedProcess(Channel channel)
    {
        return _sonarProvider.GetRoutedProcess(channel);
    }

    #endregion

    #region Commands

    public void SetMode(Mode mode)
    {
        _sonarCommand.SetMode(mode);
    }

    public void SetChatMixBalance(double balance)
    {
        _sonarCommand.SetChatMixBalance(balance);
    }
    
    public void SetRedirectionState(bool newState, Channel channel, Mix mix)
    {
        _sonarCommand.SetRedirectionState(newState, channel, mix);
    }

    public void SetAudienceMonitoringState(bool newState)
    {
        _sonarCommand.SetAudienceMonitoringState(newState);
    }

    public void SetProcessToDeviceRouting(int pId, Channel channel)
    {
        _sonarCommand.SetProcessToDeviceRouting(pId, channel);
    }

    #endregion
}