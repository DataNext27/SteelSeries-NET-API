﻿using System.Collections;
using System.Text.Json;
using SteelSeriesAPI.Interfaces;
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

    public double GetVolume(Device device)
    {
        JsonDocument volumeSettings = new HttpProvider("volumeSettings/classic/").Provide();

        if (device == Device.Master)
            return volumeSettings.RootElement.GetProperty("masters").GetProperty("classic").GetProperty("volume").GetDouble();
        return volumeSettings.RootElement.GetProperty("devices").GetProperty(device.ToDictKey()).GetProperty("classic").GetProperty("volume").GetDouble();
    }

    public double GetVolume(Device device, Channel channel)
    {
        JsonDocument volumeSettings = new HttpProvider("volumeSettings/streamer/").Provide();

        if (device == Device.Master)
            return volumeSettings.RootElement.GetProperty("masters").GetProperty("stream").GetProperty(channel.ToDictKey()).GetProperty("volume").GetDouble();
        return volumeSettings.RootElement.GetProperty("devices").GetProperty(device.ToDictKey()).GetProperty("stream").GetProperty(channel.ToDictKey()).GetProperty("volume").GetDouble();
    }

    public bool GetMute(Device device)
    {
        JsonDocument volumeSettings = new HttpProvider("volumeSettings/classic/").Provide();

        if (device == Device.Master)
            return volumeSettings.RootElement.GetProperty("masters").GetProperty("classic").GetProperty("muted").GetBoolean();
        return volumeSettings.RootElement.GetProperty("devices").GetProperty(device.ToDictKey()).GetProperty("classic").GetProperty("muted").GetBoolean();
    }

    public bool GetMute(Device device, Channel channel)
    {
        JsonDocument volumeSettings = new HttpProvider("volumeSettings/streamer/").Provide();

        if (device == Device.Master)
            return volumeSettings.RootElement.GetProperty("masters").GetProperty("stream").GetProperty(channel.ToDictKey()).GetProperty("muted").GetBoolean();
        return volumeSettings.RootElement.GetProperty("devices").GetProperty(device.ToDictKey()).GetProperty("stream").GetProperty(channel.ToDictKey()).GetProperty("muted").GetBoolean();
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
            
            yield return new SonarAudioConfiguration(id, name, (Device)DeviceExtensions.FromDictKey(vDevice));
        }
    }
    
    public IEnumerable<SonarAudioConfiguration> GetAudioConfigurations(Device device)
    {
        if (device == Device.Master)
        {
            throw new Exception("Can't get audio configurations for master");
        }
        
        IEnumerable<SonarAudioConfiguration> configs = GetAllAudioConfigurations();
        List<SonarAudioConfiguration> deviceConfigs = new List<SonarAudioConfiguration>();
        
        foreach (var config in configs)
        {
            if (config.AssociatedDevice == device)
            {
                deviceConfigs.Add(config);
            }
        }

        return deviceConfigs.OrderBy(s => s.Name);
    }
    
    public SonarAudioConfiguration GetSelectedAudioConfiguration(Device device)
    {
        if (device == Device.Master)
        {
            throw new Exception("Can't get audio configuration for master");
        }

        JsonDocument selectedConfigs = new HttpProvider("configs/selected").Provide();
        JsonElement sConfig = default;

        foreach (var config in selectedConfigs.RootElement.EnumerateArray())
        {
            if (config.GetProperty("virtualAudioDevice").GetString() == device.ToDictKey())
            {
                sConfig = config;
                break;
            }
        }

        string id = sConfig.GetProperty("id").GetString();
        string name = sConfig.GetProperty("name").GetString();
        string vDevice = sConfig.GetProperty("virtualAudioDevice").GetString();

        return new SonarAudioConfiguration(id, name, (Device)DeviceExtensions.FromDictKey(vDevice));
    }
    
    public Device GetDeviceFromAudioConfigurationId(string configId)
    {
        var configs = GetAllAudioConfigurations();
        SonarAudioConfiguration sonarConfig = null;
        foreach (var config in configs)
        {
            if (config.Id == configId)
            {
                sonarConfig = config;
                break;
            }
        }

        return sonarConfig.AssociatedDevice;
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

    public IEnumerable<RedirectionDevice> GetRedirectionDevices(Direction direction)
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
            List<Device> associatedClassicDevices = new List<Device>();
            ArrayList associatedStreamDevices = new ArrayList();
            
            GetAssociatedDevices(id, associatedClassicDevices, associatedStreamDevices);
    
            if (dataFlow == direction.ToDictKey())
            {
                yield return new RedirectionDevice(id, name, (Direction)DirectionExtensions.FromDictKey(dataFlow), associatedClassicDevices, associatedStreamDevices);
            }
        }
    }

    public RedirectionDevice GetClassicRedirectionDevice(Device device)
    {
        if (device == Device.Master)
        {
            throw new Exception("Can't get redirection device for master");
        }
        
        JsonDocument classicRedirections = new HttpProvider("classicRedirections").Provide();
        JsonElement cRedirections = default;
    
        foreach (var element in classicRedirections.RootElement.EnumerateArray())
        {
            if (element.GetProperty("id").GetString() == device.ToDictKey(DeviceMapChoice.DeviceDict))
            {
                cRedirections = element;
                break;
            }
        }
        
        string deviceId = cRedirections.GetProperty("deviceId").GetString();
        List<Device> associatedClassicDevices = new List<Device>();
        ArrayList associatedStreamDevices = new ArrayList();
        
        GetAssociatedDevices(deviceId, associatedClassicDevices, associatedStreamDevices);
    
        JsonDocument audioDevice = new HttpProvider("audioDevices/" + deviceId).Provide();
    
        string name = audioDevice.RootElement.GetProperty("friendlyName").GetString();
        Direction dataFlow = (Direction)DirectionExtensions.FromDictKey(audioDevice.RootElement.GetProperty("dataFlow").GetString());
    
        return new RedirectionDevice(deviceId, name, dataFlow, associatedClassicDevices, associatedStreamDevices);
    }

    public RedirectionDevice GetStreamRedirectionDevice(Channel channel)
    {
        JsonDocument streamRedirections = new HttpProvider("streamRedirections").Provide();
        JsonElement sRedirections = default;
    
        foreach (var element in streamRedirections.RootElement.EnumerateArray())
        {
            if (element.GetProperty("streamRedirectionId").GetString() == channel.ToDictKey())
            {
                sRedirections = element;
                break;
            }
        }
        
        string deviceId = sRedirections.GetProperty("deviceId").GetString();
        List<Device> associatedClassicDevices = new List<Device>();ArrayList associatedStreamDevices = new ArrayList();

        GetAssociatedDevices(deviceId, associatedClassicDevices, associatedStreamDevices);
    
        JsonDocument audioDevice = new HttpProvider("audioDevices/" + deviceId).Provide();
    
        string name = audioDevice.RootElement.GetProperty("friendlyName").GetString();
        Direction dataFlow = (Direction)DirectionExtensions.FromDictKey(audioDevice.RootElement.GetProperty("dataFlow").GetString());
    
        return new RedirectionDevice(deviceId, name, dataFlow, associatedClassicDevices, associatedStreamDevices);
    }
    
    public RedirectionDevice GetStreamRedirectionDevice(Device device)
    {
        if (device != Device.Mic)
        {
            throw new Exception("Can only get stream redirection device for Mic");
        }
        
        JsonDocument streamRedirections = new HttpProvider("streamRedirections").Provide();
        JsonElement sRedirections = default;
    
        foreach (var element in streamRedirections.RootElement.EnumerateArray())
        {
            if (element.GetProperty("streamRedirectionId").GetString() == device.ToDictKey(DeviceMapChoice.DeviceDict))
            {
                sRedirections = element;
                break;
            }
        }
        
        string deviceId = sRedirections.GetProperty("deviceId").GetString();
        List<Device> associatedClassicDevices = new List<Device>();
        ArrayList associatedStreamDevices = new ArrayList();
        
        GetAssociatedDevices(deviceId, associatedClassicDevices, associatedStreamDevices);
    
        JsonDocument audioDevice = new HttpProvider("audioDevices/" + deviceId).Provide();
    
        string name = audioDevice.RootElement.GetProperty("friendlyName").GetString();
        Direction dataFlow = (Direction)DirectionExtensions.FromDictKey(audioDevice.RootElement.GetProperty("dataFlow").GetString());
    
        return new RedirectionDevice(deviceId, name, dataFlow, associatedClassicDevices, associatedStreamDevices);
    }

    public RedirectionDevice GetRedirectionDeviceFromId(string deviceId)
    {
        try
        {
            JsonElement device = new HttpProvider("audioDevices/" + deviceId).Provide().RootElement;
            
            string id = device.GetProperty("id").GetString();
            string name = device.GetProperty("friendlyName").GetString();
            string dataFlow = device.GetProperty("dataFlow").GetString();
            List<Device> associatedClassicDevices = new List<Device>();
            ArrayList associatedStreamDevices = new ArrayList();
        
            GetAssociatedDevices(deviceId, associatedClassicDevices, associatedStreamDevices);
        
            return new RedirectionDevice(id, name, (Direction)DirectionExtensions.FromDictKey(dataFlow), associatedClassicDevices, associatedStreamDevices);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception("Can't get any device from this Id, maybe the device doesn't exist or its Id changed.");
        }
    }

    private void GetAssociatedDevices(string deviceId, List<Device> associatedClassicDevices, ArrayList associatedStreamDevices)
    {
        JsonDocument classicRedirections = new HttpProvider("classicRedirections").Provide();
        JsonDocument streamRedirections = new HttpProvider("streamRedirections").Provide();

        foreach (var element in classicRedirections.RootElement.EnumerateArray())
        {
            if (element.GetProperty("deviceId").GetString() == deviceId)
            {
                associatedClassicDevices.Add((Device)DeviceExtensions.FromDictKey(element.GetProperty("id").GetString(), DeviceMapChoice.DeviceDict));
            }
        }
        
        foreach (var element in streamRedirections.RootElement.EnumerateArray())
        {
            if (element.GetProperty("deviceId").GetString() == deviceId)
            {
                if (element.GetProperty("streamRedirectionId").GetString() == "mic")
                {
                    associatedStreamDevices.Add((Device)DeviceExtensions.FromDictKey(element.GetProperty("streamRedirectionId").GetString(), DeviceMapChoice.DeviceDict));
                }
                else
                {
                    associatedStreamDevices.Add((Channel)ChannelExtensions.FromDictKey(element.GetProperty("streamRedirectionId").GetString()));
                }
            }
        }
    }

    public bool GetRedirectionState(Device device, Channel channel)
    {
        if (device == Device.Master)
        {
            throw new Exception("Can't get redirection state for master");
        }

        JsonDocument streamRedirections = new HttpProvider("streamRedirections").Provide();
        JsonElement streamChannel = default;

        foreach (var element in streamRedirections.RootElement.EnumerateArray())
        {
            if (element.GetProperty("streamRedirectionId").GetString() == channel.ToDictKey())
            {
                streamChannel = element;
                break;
            }
        }

        JsonElement status = default;

        foreach (var element in streamChannel.GetProperty("status").EnumerateArray())
        {
            if (element.GetProperty("role").GetString() == device.ToDictKey())
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

    public IEnumerable<RoutedProcess> GetRoutedProcess(Device device)
    {
        if (device == Device.Master)
        {
            throw new Exception("Can't get routed process for Master");
        }
        
        JsonDocument audioDeviceRoutings = new HttpProvider("AudioDeviceRouting").Provide();

        foreach (var element in audioDeviceRoutings.RootElement.EnumerateArray())
        {
            if (element.GetProperty("role").GetString() != device.ToDictKey())
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

                if (processName == "Idle" && displayName == "Idle" && state == RoutedProcessState.Inactive && pId == 0)
                {
                    continue;
                }
                
                yield return new RoutedProcess(id, processName, pId, state, displayName);
            }
        }
    }
}