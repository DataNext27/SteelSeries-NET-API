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
    
    public VolumeSettings GetVolumeSetting(Device device, Mode mode, Channel channel)
    { 
        JsonDocument volumeSettings = new HttpProvider($"volumeSettings/{mode.ToDictKey(ModeMapChoice.StreamerDict)}").Provide();
        JsonElement vSetting;
        
        if (device == Device.Master)
        {
            vSetting = volumeSettings.RootElement.GetProperty("masters").GetProperty(mode.ToDictKey(ModeMapChoice.StreamDict));
        }
        else
        {
            vSetting = volumeSettings.RootElement.GetProperty("devices").GetProperty(device.ToDictKey()).GetProperty(mode.ToDictKey(ModeMapChoice.StreamDict));
        }
        
        if (mode == Mode.Streamer) vSetting = vSetting.GetProperty(channel.ToDictKey());
        
        double volume = vSetting.GetProperty("volume").GetDouble();
        bool mute = vSetting.GetProperty("muted").GetBoolean();
        
        return new VolumeSettings(volume, mute);
    }
    
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
            throw new Exception("Can't get audio configurations for master");
        }

        JsonDocument selectedConfigs = new HttpProvider("configs/selected").Provide();
        JsonElement sConfig = default;

        foreach (var config in selectedConfigs.RootElement.EnumerateArray())
        {
            if (config.GetProperty("virtualAudioDevice").GetString() == device.ToDictKey())
            {
                sConfig = config;
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
            }
        }

        return sonarConfig.AssociatedDevice;
    }
}