using System.Globalization;
using System.Text.Json;
using SteelSeriesAPI.Sonar.Interfaces;
using SteelSeriesAPI.Sonar.Enums;

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
        new HttpPut("mode/" + mode.ToDictKey(ModeMapChoice.StreamDict));
        Thread.Sleep(100); // Prevent bugs/freezes/crashes
    }

    public void SetVolume(double vol, Channel channel)
    {
        string _vol = vol.ToString("0.00", CultureInfo.InvariantCulture);
        new HttpPut("volumeSettings/classic/" + channel.ToDictKey(ChannelMapChoice.HttpDict) + "/Volume/" + _vol);
    }

    public void SetVolume(double vol, Channel channel, Mix mix)
    {
        string _vol = vol.ToString("0.00", CultureInfo.InvariantCulture);
        new HttpPut("volumeSettings/streamer/" + mix.ToDictKey() + "/" + channel.ToDictKey(ChannelMapChoice.HttpDict) + "/volume/" + _vol);
    }

    public void SetMute(bool mute, Channel channel)
    {
        new HttpPut("volumeSettings/classic/" + channel.ToDictKey(ChannelMapChoice.HttpDict) + "/Mute/" + mute);
    }

    public void SetMute(bool mute, Channel channel, Mix mix)
    {
        new HttpPut("volumeSettings/streamer/" + mix.ToDictKey() + "/" + channel.ToDictKey(ChannelMapChoice.HttpDict) + "/isMuted/" + mute);
    }

    public void SetConfig(string configId)
    {
        if (string.IsNullOrEmpty(configId)) throw new Exception("Couldn't retrieve config id");

        new HttpPut("configs/" + configId + "/select");
    }

    public void SetConfig(Channel channel, string name)
    {
        var configs = _sonarBridge.GetAudioConfigurations(channel).ToList();
        foreach (var config in configs)
        {
            if (config.Name == name)
            {
                SetConfig(config.Id);
                break;
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

    public void SetClassicPlaybackDevice(string deviceId, Channel channel)
    {
        new HttpPut("classicRedirections/" + channel.ToDictKey(ChannelMapChoice.ChannelDict) +"/deviceId/" + deviceId);
    }
    
    public void SetStreamPlaybackDevice(string deviceId, Mix mix)
    {
        new HttpPut("streamRedirections/" + mix.ToDictKey() +"/deviceId/" + deviceId);
    }
    
    public void SetStreamPlaybackDevice(string deviceId, Channel channel)
    {
        if (channel != Channel.MIC)
        {
            throw new Exception("Can only change stream redirection channel for Mic");
        }
        
        new HttpPut("streamRedirections/" + channel.ToDictKey(ChannelMapChoice.ChannelDict) +"/deviceId/" + deviceId);
    }

    public void SetRedirectionState(bool newState, Channel channel, Mix mix)
    {
        new HttpPut("streamRedirections/" + mix.ToDictKey() + "/redirections/" + channel.ToDictKey() +
                    "/isEnabled/" + newState);
    }

    public void SetAudienceMonitoringState(bool newState)
    {
        new HttpPut("streamRedirections/isStreamMonitoringEnabled/" + newState);
    }
    
    public void SetProcessToDeviceRouting(int pId, Channel channel)
    {
        if (channel == Channel.MASTER)
        {
            throw new Exception("Can't set process to master routing");
        }
        
        JsonDocument audioDeviceRouting = new HttpProvider("AudioDeviceRouting").Provide();

        foreach (var element in audioDeviceRouting.RootElement.EnumerateArray())
        {
            if (element.GetProperty("role").GetString() == channel.ToDictKey())
            {
                if (channel == Channel.MIC)
                {
                    new HttpPut("AudioDeviceRouting/capture/" + element.GetProperty("deviceId").GetString() + "/" + pId);
                    break;
                }
                else
                {
                    new HttpPut("AudioDeviceRouting/render/" + element.GetProperty("deviceId").GetString() + "/" + pId);
                    break;
                }
            }
        }
    }
}