using System.Globalization;
using System.Text.Json;
using SteelSeriesAPI.Interfaces;
using SteelSeriesAPI.Sonar.Enums;
using Channel = SteelSeriesAPI.Sonar.Enums.Channel;

namespace SteelSeriesAPI.Sonar.Http;

public class SonarHttpCommand : ISonarCommandHandler
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
        string target = mode.ToDictKey() + "/";
        
        if (mode == Mode.Streamer)
        {
            target += channel.ToDictKey() + "/" + device.ToDictKey(DeviceMapChoice.HttpDict) + "/volume/" + _vol;
        }
        else
        {
            target += device.ToDictKey(DeviceMapChoice.HttpDict) + "/Volume/" + _vol;
        }
        Console.WriteLine("volumeSettings/" + target);
        new HttpPut("volumeSettings/" + target);
    }
    
    public void SetMute(bool mute, Device device, Mode mode, Channel channel)
    {
        string target = mode.ToDictKey() + "/";
        if (mode == Mode.Streamer)
        {
            target += channel.ToDictKey() + "/" + device.ToDictKey(DeviceMapChoice.HttpDict) + "/isMuted/" + mute;
        }
        else
        {
            target += device.ToDictKey(DeviceMapChoice.HttpDict) + "/Mute/" + mute;
        }

        new HttpPut("volumeSettings/" + target);
    }

    public void SetConfig(string configId)
    {
        if (string.IsNullOrEmpty(configId)) throw new Exception("Couldn't retrieve config id");

        new HttpPut("configs/" + configId + "/select");
    }

    public void SetConfig(Device device, string name)
    {
        var configs = _sonarBridge.GetAudioConfigurations(device).ToList();
        foreach (var config in configs)
        {
            if (config.Name  == name)
            {
                SetConfig(config.Id);
            }
        }
    }

    public void SetChatMixBalance(double balance)
    {
        if (!_sonarBridge.GetChatMixState())
        {
            throw new Exception("Can't change the value of the balance of the ChatMix when it is not enabled");
        }

        if (balance > 1 || balance < -1)
        {
            throw new Exception("ChatMix balance can't be less than -1 and greater than 1");
        }

        new HttpPut("chatMix?balance=" + balance.ToString("0.00", CultureInfo.InvariantCulture));
    }

    public void SetClassicRedirectionDevice(string deviceId, Device device)
    {
        new HttpPut("classicRedirections/" + device.ToDictKey(DeviceMapChoice.DeviceDict) +"/deviceId/" + deviceId);
    }
    
    public void SetStreamRedirectionDevice(string deviceId, Channel channel)
    {
        new HttpPut("streamRedirections/" + channel.ToDictKey() +"/deviceId/" + deviceId);
    }
    
    public void SetStreamRedirectionDevice(string deviceId, Device device)
    {
        if (device != Device.Mic)
        {
            throw new Exception("Can only get stream redirection device for Mic");
        }
        
        new HttpPut("streamRedirections/" + device.ToDictKey(DeviceMapChoice.DeviceDict) +"/deviceId/" + deviceId);
    }

    public void SetRedirectionState(bool newState, Device device, Channel channel)
    {
        new HttpPut("streamRedirections/" + channel.ToDictKey() + "/redirections/" + device.ToDictKey() +
                    "/isEnabled/" + newState);
    }

    public void SetAudienceMonitoringState(bool newState)
    {
        new HttpPut("streamRedirections/isStreamMonitoringEnabled/" + newState);
    }
    
    public void SetProcessToDeviceRouting(int pid, Device device)
    {
        if (device == Device.Master)
        {
            throw new Exception("Can't set process to master routing");
        }
        
        JsonDocument audioDeviceRouting = new HttpProvider("AudioDeviceRouting").Provide();

        foreach (var element in audioDeviceRouting.RootElement.EnumerateArray())
        {
            if (element.GetProperty("role").GetString() == device.ToDictKey())
            {
                if (device == Device.Mic)
                {
                    new HttpPut("AudioDeviceRouting/capture/" + element.GetProperty("deviceId").GetString() + "/" + pid);
                    break;
                }
                else
                {
                    new HttpPut("AudioDeviceRouting/render/" + element.GetProperty("deviceId").GetString() + "/" + pid);
                    break;
                }
            }
        }
    }
}