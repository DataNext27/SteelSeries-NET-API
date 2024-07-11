using System.Globalization;
using SteelSeriesAPI.Interfaces;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Http;

public class SonarHttpCommand
{
    private readonly ISonarBridge _sonarBridge;
    
    public SonarHttpCommand(SonarBridge sonarBridge)
    {
        _sonarBridge = sonarBridge;
    }
    
    public void SetMode(Mode mode)
    {
        if (mode == _sonarBridge.GetMode())
        {
            throw new Exception("Already using this mode");
        }

        if (mode == Mode.Classic) new HttpPut("mode/classic");
        if (mode == Mode.Streamer) new HttpPut("mode/stream");
        Thread.Sleep(100);
    }
    
    public void SetVolume(double vol, Device device, Mode mode, Channel channel)
    {
        string _vol = vol.ToString("0.00", CultureInfo.InvariantCulture);
        string target = mode.ToJsonKey() + "/";
        
        if (mode == Mode.Streamer)
        {
            target += channel.ToJsonKey() + "/" + device.ToJsonKey(DeviceMapChoice.HttpDict) + "/volume/" + _vol;
        }
        else
        {
            target += device.ToJsonKey(DeviceMapChoice.HttpDict) + "/Volume/" + _vol;
        }
        Console.WriteLine("volumeSettings/" + target);
        new HttpPut("volumeSettings/" + target);
    }
    
    public void SetMute(bool mute, Device device, Mode mode, Channel channel)
    {
        string target = mode.ToJsonKey() + "/";
        if (mode == Mode.Streamer)
        {
            target += channel.ToJsonKey() + "/" + device.ToJsonKey(DeviceMapChoice.HttpDict) + "/isMuted/" + mute;
        }
        else
        {
            target += device.ToJsonKey(DeviceMapChoice.HttpDict) + "/Mute/" + mute;
        }

        new HttpPut("volumeSettings/" + target);
    }
}