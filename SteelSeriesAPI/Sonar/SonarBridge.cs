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
    public readonly SonarEventManager SonarEventManager;

    private string _sonarWebServerAddress;

    public SonarBridge()
    {
        _sonarRetriever = SonarRetriever.Instance;
        WaitUntilSonarStarted();
        _sonarWebServerAddress = _sonarRetriever.WebServerAddress();
        SonarEventManager = new SonarEventManager();
        _sonarSocket = new SonarSocket(_sonarWebServerAddress, SonarEventManager);
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
    public double GetVolume(Device device)
    {
        return _sonarProvider.GetVolume(device);
    }

    public double GetVolume(Device device, Channel channel)
    {
        return _sonarProvider.GetVolume(device, channel);
    }

    public bool GetMute(Device device)
    {
        return _sonarProvider.GetMute(device);
    }

    public bool GetMute(Device device, Channel channel)
    {
        return _sonarProvider.GetMute(device, channel);
    }

    public IEnumerable<SonarAudioConfiguration> GetAllAudioConfigurations()
    {
        return _sonarProvider.GetAllAudioConfigurations();
    }

    public SonarAudioConfiguration GetAudioConfiguration(string configId)
    {
        return _sonarProvider.GetAudioConfiguration(configId);
    }

    public IEnumerable<SonarAudioConfiguration> GetAudioConfigurations(Device device)
    {
        return _sonarProvider.GetAudioConfigurations(device);
    }

    public SonarAudioConfiguration GetSelectedAudioConfiguration(Device device)
    {
        return _sonarProvider.GetSelectedAudioConfiguration(device);
    }
    
    public double GetChatMixBalance()
    {
        return _sonarProvider.GetChatMixBalance();
    }

    public bool GetChatMixState()
    {
        return _sonarProvider.GetChatMixState();
    }

    public IEnumerable<RedirectionDevice> GetRedirectionDevices(Direction direction)
    {
        return _sonarProvider.GetRedirectionDevices(direction);
    }

    public RedirectionDevice GetClassicRedirectionDevice(Device device)
    {
        return _sonarProvider.GetClassicRedirectionDevice(device);
    }

    public RedirectionDevice GetStreamRedirectionDevice(Channel channel)
    {
        return _sonarProvider.GetStreamRedirectionDevice(channel);
    }

    public RedirectionDevice GetStreamRedirectionDevice(Device device = Device.Mic)
    {
        return _sonarProvider.GetStreamRedirectionDevice(device);
    }

    public RedirectionDevice GetRedirectionDeviceFromId(string deviceId)
    {
        return _sonarProvider.GetRedirectionDeviceFromId(deviceId);
    }

    public bool GetRedirectionState(Device device, Channel channel)
    {
        return _sonarProvider.GetRedirectionState(device, channel);
    }

    public bool GetAudienceMonitoringState()
    {
        return _sonarProvider.GetAudienceMonitoringState();
    }

    public IEnumerable<RoutedProcess> GetRoutedProcess(Device device)
    {
        return _sonarProvider.GetRoutedProcess(device);
    }

    #endregion

    #region Commands

    public void SetMode(Mode mode)
    {
        _sonarCommand.SetMode(mode);
    }

    public void SetVolume(double vol, Device device)
    {
        _sonarCommand.SetVolume(vol, device);
    }

    public void SetVolume(double vol, Device device, Channel channel)
    {
        _sonarCommand.SetVolume(vol, device, channel);
    }
    
    public void SetMute(bool mute, Device device)
    {
        _sonarCommand.SetMute(mute, device);
    }

    public void SetMute(bool mute, Device device, Channel channel)
    {
        _sonarCommand.SetMute(mute, device, channel);
    }

    public void SetConfig(string configId)
    {
        _sonarCommand.SetConfig(configId);
    }
    
    public void SetConfig(Device device, string name)
    {
        _sonarCommand.SetConfig(device, name);
    }

    public void SetChatMixBalance(double balance)
    {
        _sonarCommand.SetChatMixBalance(balance);
    }

    public void SetClassicRedirectionDevice(string deviceId, Device device)
    {
        _sonarCommand.SetClassicRedirectionDevice(deviceId, device);
    }

    public void SetStreamRedirectionDevice(string deviceId, Channel channel)
    {
        _sonarCommand.SetStreamRedirectionDevice(deviceId, channel);
    }

    public void SetStreamRedirectionDevice(string deviceId, Device device = Device.Mic)
    {
        _sonarCommand.SetStreamRedirectionDevice(deviceId, device);
    }
    
    public void SetRedirectionState(bool newState, Device device, Channel channel)
    {
        _sonarCommand.SetRedirectionState(newState, device, channel);
    }

    public void SetAudienceMonitoringState(bool newState)
    {
        _sonarCommand.SetAudienceMonitoringState(newState);
    }

    public void SetProcessToDeviceRouting(int pId, Device device)
    {
        _sonarCommand.SetProcessToDeviceRouting(pId, device);
    }

    #endregion
}