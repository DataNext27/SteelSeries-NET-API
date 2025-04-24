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
    public readonly EventManager EventManager;

    private string _sonarWebServerAddress;

    public SonarBridge()
    {
        _sonarRetriever = SonarRetriever.Instance;
        WaitUntilSonarStarted();
        _sonarWebServerAddress = _sonarRetriever.WebServerAddress();
        EventManager = new EventManager();
        _sonarSocket = new SonarSocket(_sonarWebServerAddress, EventManager);
        _sonarCommand = new SonarHttpCommand(this);
        _sonarProvider = new SonarHttpProvider(this);
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
    
    // volume = 0,00000000 <-- 8 decimal max
    public double GetVolume(Channel channel)
    {
        return _sonarProvider.GetVolume(channel);
    }

    public double GetVolume(Channel channel, Mix mix)
    {
        return _sonarProvider.GetVolume(channel, mix);
    }

    public bool GetMute(Channel channel)
    {
        return _sonarProvider.GetMute(channel);
    }

    public bool GetMute(Channel channel, Mix mix)
    {
        return _sonarProvider.GetMute(channel, mix);
    }

    public IEnumerable<SonarAudioConfiguration> GetAllAudioConfigurations()
    {
        return _sonarProvider.GetAllAudioConfigurations();
    }

    public SonarAudioConfiguration GetAudioConfiguration(string configId)
    {
        return _sonarProvider.GetAudioConfiguration(configId);
    }

    public IEnumerable<SonarAudioConfiguration> GetAudioConfigurations(Channel channel)
    {
        return _sonarProvider.GetAudioConfigurations(channel);
    }

    public SonarAudioConfiguration GetSelectedAudioConfiguration(Channel channel)
    {
        return _sonarProvider.GetSelectedAudioConfiguration(channel);
    }
    
    public double GetChatMixBalance()
    {
        return _sonarProvider.GetChatMixBalance();
    }

    public bool GetChatMixState()
    {
        return _sonarProvider.GetChatMixState();
    }

    public IEnumerable<PlaybackDevice> GetPlaybackDevices(DataFlow dataFlow)
    {
        return _sonarProvider.GetPlaybackDevices(dataFlow);
    }

    public PlaybackDevice GetClassicPlaybackDevice(Channel channel)
    {
        return _sonarProvider.GetClassicPlaybackDevice(channel);
    }

    public PlaybackDevice GetStreamPlaybackDevice(Mix mix)
    {
        return _sonarProvider.GetStreamPlaybackDevice(mix);
    }

    public PlaybackDevice GetStreamPlaybackDevice(Channel channel = Channel.MIC)
    {
        return _sonarProvider.GetStreamPlaybackDevice(channel);
    }

    public PlaybackDevice GetPlaybackDeviceFromId(string deviceId)
    {
        return _sonarProvider.GetPlaybackDeviceFromId(deviceId);
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

    public void SetVolume(double vol, Channel channel)
    {
        _sonarCommand.SetVolume(vol, channel);
    }

    public void SetVolume(double vol, Channel channel, Mix mix)
    {
        _sonarCommand.SetVolume(vol, channel, mix);
    }
    
    public void SetMute(bool mute, Channel channel)
    {
        _sonarCommand.SetMute(mute, channel);
    }

    public void SetMute(bool mute, Channel channel, Mix mix)
    {
        _sonarCommand.SetMute(mute, channel, mix);
    }

    public void SetConfig(string configId)
    {
        _sonarCommand.SetConfig(configId);
    }
    
    public void SetConfig(Channel channel, string name)
    {
        _sonarCommand.SetConfig(channel, name);
    }

    public void SetChatMixBalance(double balance)
    {
        _sonarCommand.SetChatMixBalance(balance);
    }

    public void SetClassicPlaybackDevice(string deviceId, Channel channel)
    {
        _sonarCommand.SetClassicPlaybackDevice(deviceId, channel);
    }

    public void SetStreamPlaybackDevice(string deviceId, Mix mix)
    {
        _sonarCommand.SetStreamPlaybackDevice(deviceId, mix);
    }

    public void SetStreamPlaybackDevice(string deviceId, Channel channel = Channel.MIC)
    {
        _sonarCommand.SetStreamPlaybackDevice(deviceId, channel);
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