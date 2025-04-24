using System.Collections;
using System.Text.Json;
using SteelSeriesAPI.Sonar.Interfaces;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Http;

public class SonarHttpProvider : ISonarDataProvider
{
    private readonly ISonarBridge _sonarBridge;
    
    public SonarHttpProvider(SonarBridge sonarBridge)
    {
        _sonarBridge = sonarBridge;
    }

    public Mode GetMode()
    {
        string mode = new HttpProvider("mode").Provide().RootElement.ToString();
        
        return (Mode)ModeExtensions.FromDictKey(mode, ModeMapChoice.StreamDict);
    }

    #region AudioConfigs
    
    public IEnumerable<SonarAudioConfiguration> GetAllAudioConfigurations()
    {
        JsonDocument configs = new HttpProvider("configs").Provide();

        foreach (var element in configs.RootElement.EnumerateArray())
        {
            string vDevice = element.GetProperty("virtualAudioDevice").GetString();
            string id = element.GetProperty("id").GetString();
            string name = element.GetProperty("name").GetString();
            
            yield return new SonarAudioConfiguration(id, name, (Channel)ChannelExtensions.FromDictKey(vDevice));
        }
    }

    public SonarAudioConfiguration GetAudioConfiguration(string configId)
    {
        IEnumerable<SonarAudioConfiguration> configs = GetAllAudioConfigurations();
        SonarAudioConfiguration sonarConfig = null;
        
        foreach (var config in configs)
        {
            if (config.Id == configId)
            {
                sonarConfig = config;
                break;
            }
        }

        return sonarConfig;
    }
    
    public IEnumerable<SonarAudioConfiguration> GetAudioConfigurations(Channel channel)
    {
        if (channel == Channel.MASTER)
        {
            throw new Exception("Can't get audio configurations for master");
        }
        
        IEnumerable<SonarAudioConfiguration> configs = GetAllAudioConfigurations();
        List<SonarAudioConfiguration> deviceConfigs = new List<SonarAudioConfiguration>();
        
        foreach (var config in configs)
        {
            if (config.AssociatedChannel == channel)
            {
                deviceConfigs.Add(config);
            }
        }

        return deviceConfigs.OrderBy(s => s.Name);
    }
    
    public SonarAudioConfiguration GetSelectedAudioConfiguration(Channel channel)
    {
        if (channel == Channel.MASTER)
        {
            throw new Exception("Can't get audio configuration for master");
        }

        JsonDocument selectedConfigs = new HttpProvider("configs/selected").Provide();
        JsonElement sConfig = default;

        foreach (var config in selectedConfigs.RootElement.EnumerateArray())
        {
            if (config.GetProperty("virtualAudioDevice").GetString() == channel.ToDictKey())
            {
                sConfig = config;
                break;
            }
        }

        string id = sConfig.GetProperty("id").GetString();
        string name = sConfig.GetProperty("name").GetString();
        string vDevice = sConfig.GetProperty("virtualAudioDevice").GetString();

        return new SonarAudioConfiguration(id, name, (Channel)ChannelExtensions.FromDictKey(vDevice));
    }
    
    #endregion

    #region ChatMix

    public double GetChatMixBalance()
    {
        JsonDocument chatMix = new HttpProvider("chatMix").Provide();

        return chatMix.RootElement.GetProperty("balance").GetDouble();
    }

    public bool GetChatMixState()
    {
        JsonDocument chatMix = new HttpProvider("chatMix").Provide();
        string cState = chatMix.RootElement.GetProperty("state").ToString();
        if (cState == "enabled")
        {
            return true;
        }
        else if (cState == "differentDeviceSelected")
        {
            return false;
        }

        return false;
    }
    
    #endregion

    public IEnumerable<PlaybackDevice> GetPlaybackDevices(DataFlow _dataFlow)
    {
        JsonDocument audioDevices = new HttpProvider("audioDevices").Provide();
        JsonDocument classicRedirections = new HttpProvider("classicRedirections").Provide();
        JsonDocument streamRedirections = new HttpProvider("streamRedirections").Provide();
    
        foreach (var element in audioDevices.RootElement.EnumerateArray())
        {
            if (element.GetProperty("role").GetString() != "none")
            {
                continue;
            }
            
            string id = element.GetProperty("id").GetString();
            string name = element.GetProperty("friendlyName").GetString();
            string dataFlow = element.GetProperty("dataFlow").GetString();
            List<Channel> associatedClassicDevices = new List<Channel>();
            ArrayList associatedStreamDevices = new ArrayList();
            
            GetAssociatedDevices(id, associatedClassicDevices, associatedStreamDevices);
    
            if (dataFlow == _dataFlow.ToDictKey())
            {
                yield return new PlaybackDevice(id, name, (DataFlow)DataFlowExtensions.FromDictKey(dataFlow), associatedClassicDevices, associatedStreamDevices);
            }
        }
    }

    public PlaybackDevice GetClassicPlaybackDevice(Channel channel)
    {
        if (channel == Channel.MASTER)
        {
            throw new Exception("Can't get redirection channel for master");
        }
        
        JsonDocument classicRedirections = new HttpProvider("classicRedirections").Provide();
        JsonElement cRedirections = default;
    
        foreach (var element in classicRedirections.RootElement.EnumerateArray())
        {
            if (element.GetProperty("id").GetString() == channel.ToDictKey(ChannelMapChoice.ChannelDict))
            {
                cRedirections = element;
                break;
            }
        }
        
        string deviceId = cRedirections.GetProperty("deviceId").GetString();
        List<Channel> associatedClassicDevices = new List<Channel>();
        ArrayList associatedStreamDevices = new ArrayList();
        
        GetAssociatedDevices(deviceId, associatedClassicDevices, associatedStreamDevices);
    
        JsonDocument audioDevice = new HttpProvider("audioDevices/" + deviceId).Provide();
    
        string name = audioDevice.RootElement.GetProperty("friendlyName").GetString();
        DataFlow dataFlow = (DataFlow)DataFlowExtensions.FromDictKey(audioDevice.RootElement.GetProperty("dataFlow").GetString());
    
        return new PlaybackDevice(deviceId, name, dataFlow, associatedClassicDevices, associatedStreamDevices);
    }

    public PlaybackDevice GetStreamPlaybackDevice(Mix mix)
    {
        JsonDocument streamRedirections = new HttpProvider("streamRedirections").Provide();
        JsonElement sRedirections = default;
    
        foreach (var element in streamRedirections.RootElement.EnumerateArray())
        {
            if (element.GetProperty("streamRedirectionId").GetString() == mix.ToDictKey())
            {
                sRedirections = element;
                break;
            }
        }
        
        string deviceId = sRedirections.GetProperty("deviceId").GetString();
        List<Channel> associatedClassicDevices = new List<Channel>();ArrayList associatedStreamDevices = new ArrayList();

        GetAssociatedDevices(deviceId, associatedClassicDevices, associatedStreamDevices);
    
        JsonDocument audioDevice = new HttpProvider("audioDevices/" + deviceId).Provide();
    
        string name = audioDevice.RootElement.GetProperty("friendlyName").GetString();
        DataFlow dataFlow = (DataFlow)DataFlowExtensions.FromDictKey(audioDevice.RootElement.GetProperty("dataFlow").GetString());
    
        return new PlaybackDevice(deviceId, name, dataFlow, associatedClassicDevices, associatedStreamDevices);
    }
    
    public PlaybackDevice GetStreamPlaybackDevice(Channel channel)
    {
        if (channel != Channel.MIC)
        {
            throw new Exception("Can only get stream redirection channel for Mic");
        }
        
        JsonDocument streamRedirections = new HttpProvider("streamRedirections").Provide();
        JsonElement sRedirections = default;
    
        foreach (var element in streamRedirections.RootElement.EnumerateArray())
        {
            if (element.GetProperty("streamRedirectionId").GetString() == channel.ToDictKey(ChannelMapChoice.ChannelDict))
            {
                sRedirections = element;
                break;
            }
        }
        
        string deviceId = sRedirections.GetProperty("deviceId").GetString();
        List<Channel> associatedClassicDevices = new List<Channel>();
        ArrayList associatedStreamDevices = new ArrayList();
        
        GetAssociatedDevices(deviceId, associatedClassicDevices, associatedStreamDevices);
    
        JsonDocument audioDevice = new HttpProvider("audioDevices/" + deviceId).Provide();
    
        string name = audioDevice.RootElement.GetProperty("friendlyName").GetString();
        DataFlow dataFlow = (DataFlow)DataFlowExtensions.FromDictKey(audioDevice.RootElement.GetProperty("dataFlow").GetString());
    
        return new PlaybackDevice(deviceId, name, dataFlow, associatedClassicDevices, associatedStreamDevices);
    }

    public PlaybackDevice GetPlaybackDeviceFromId(string deviceId)
    {
        try
        {
            JsonElement device = new HttpProvider("audioDevices/" + deviceId).Provide().RootElement;
            
            string id = device.GetProperty("id").GetString();
            string name = device.GetProperty("friendlyName").GetString();
            string dataFlow = device.GetProperty("dataFlow").GetString();
            List<Channel> associatedClassicDevices = new List<Channel>();
            ArrayList associatedStreamDevices = new ArrayList();
        
            GetAssociatedDevices(deviceId, associatedClassicDevices, associatedStreamDevices);
        
            return new PlaybackDevice(id, name, (DataFlow)DataFlowExtensions.FromDictKey(dataFlow), associatedClassicDevices, associatedStreamDevices);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception("Can't get any channel from this Id, maybe the channel doesn't exist or its Id changed.");
        }
    }

    private void GetAssociatedDevices(string deviceId, List<Channel> associatedClassicDevices, ArrayList associatedStreamDevices)
    {
        JsonDocument classicRedirections = new HttpProvider("classicRedirections").Provide();
        JsonDocument streamRedirections = new HttpProvider("streamRedirections").Provide();

        foreach (var element in classicRedirections.RootElement.EnumerateArray())
        {
            if (element.GetProperty("deviceId").GetString() == deviceId)
            {
                associatedClassicDevices.Add((Channel)ChannelExtensions.FromDictKey(element.GetProperty("id").GetString(), ChannelMapChoice.ChannelDict));
            }
        }
        
        foreach (var element in streamRedirections.RootElement.EnumerateArray())
        {
            if (element.GetProperty("deviceId").GetString() == deviceId)
            {
                if (element.GetProperty("streamRedirectionId").GetString() == "mic")
                {
                    associatedStreamDevices.Add((Channel)ChannelExtensions.FromDictKey(element.GetProperty("streamRedirectionId").GetString(), ChannelMapChoice.ChannelDict));
                }
                else
                {
                    associatedStreamDevices.Add((Mix)MixExtensions.FromDictKey(element.GetProperty("streamRedirectionId").GetString()));
                }
            }
        }
    }

    public bool GetRedirectionState(Channel channel, Mix mix)
    {
        if (channel == Channel.MASTER)
        {
            throw new Exception("Can't get redirection state for master");
        }

        JsonDocument streamRedirections = new HttpProvider("streamRedirections").Provide();
        JsonElement streamChannel = default;

        foreach (var element in streamRedirections.RootElement.EnumerateArray())
        {
            if (element.GetProperty("streamRedirectionId").GetString() == mix.ToDictKey())
            {
                streamChannel = element;
                break;
            }
        }

        JsonElement status = default;

        foreach (var element in streamChannel.GetProperty("status").EnumerateArray())
        {
            if (element.GetProperty("role").GetString() == channel.ToDictKey())
            {
                status = element;
                break;
            }
        }

        bool state = status.GetProperty("isEnabled").GetBoolean();

        return state;
    }

    public bool GetAudienceMonitoringState()
    {
        JsonDocument streamMonitoring = new HttpProvider("streamRedirections/isStreamMonitoringEnabled").Provide();

        return streamMonitoring.RootElement.GetBoolean();
    }

    public IEnumerable<RoutedProcess> GetRoutedProcess(Channel channel)
    {
        if (channel == Channel.MASTER)
        {
            throw new Exception("Can't get routed process for Master");
        }
        
        JsonDocument audioDeviceRoutings = new HttpProvider("AudioDeviceRouting").Provide();

        foreach (var element in audioDeviceRoutings.RootElement.EnumerateArray())
        {
            if (element.GetProperty("role").GetString() != channel.ToDictKey())
            {
                continue;
            }

            var audioSessions = element.GetProperty("audioSessions");

            foreach (var session in audioSessions.EnumerateArray())
            {
                string id = session.GetProperty("id").GetString().Split("|")[0];
                string processName = session.GetProperty("processName").GetString();
                int pId = session.GetProperty("processId").GetInt32();
                RoutedProcessState state = (RoutedProcessState)RoutedProcessStateExtensions.FromDictKey(session.GetProperty("state").GetString());
                string displayName = session.GetProperty("displayName").GetString();

                if (processName == "Idle" && displayName == "Idle" && state == RoutedProcessState.INACTIVE && pId == 0)
                {
                    continue;
                }
                
                yield return new RoutedProcess(id, processName, pId, state, displayName);
            }
        }
    }
}