using System.Security.Principal;
using SteelSeriesAPI.EventArgs;
using SteelSeriesAPI.Interfaces;
using SteelSeriesAPI.Sonar;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Http;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI;

public class SonarBridge : ISonarBridge
{
    private readonly IAppRetriever _sonarRetriever;
    private readonly SonarHttpCommand _sonarCommand;
    private readonly SonarHttpProvider _sonarProvider;
    private readonly ISonarSocket _sonarSocket;

    private string _sonarWebServerAddress;
    
    public event EventHandler<SonarEventArgs> OnSteelSeriesSonarEvent;

    public SonarBridge()
    {
        _sonarRetriever = new SonarRetriever();
        WaitUntilSonarStarted();
        _sonarWebServerAddress = _sonarRetriever.WebServerAddress();
        _sonarSocket = new SonarSocket(_sonarWebServerAddress);
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
    public double GetVolume(Device device, Mode mode = Mode.Classic, Channel channel = Channel.Monitoring)
    {
        return _sonarProvider.GetVolumeSetting(device, mode, channel).Volume;
    }
    
    public bool GetMute(Device device, Mode mode = Mode.Classic, Channel channel = Channel.Monitoring)
    {
        return _sonarProvider.GetVolumeSetting(device, mode, channel).Mute;
    }

    #endregion

    #region Commands

    public void SetMode(Mode mode)
    {
        _sonarCommand.SetMode(mode);
    }

    public void SetVolume(double vol, Device device, Mode mode = Mode.Classic, Channel channel = Channel.Monitoring)
    {
        _sonarCommand.SetVolume(vol, device, mode, channel);
    }

    public void SetMute(bool mute, Device device, Mode mode = Mode.Classic, Channel channel = Channel.Monitoring)
    {
        _sonarCommand.SetMute(mute, device, mode, channel);
    }

    #endregion
}