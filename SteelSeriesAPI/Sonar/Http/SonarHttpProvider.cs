using System.Text.Json;
using SteelSeriesAPI.Interfaces;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Http;

public class SonarHttpProvider
{
    private readonly ISonarBridge _sonarBridge;
    
    public SonarHttpProvider(SonarBridge sonarBridge)
    {
        _sonarBridge = sonarBridge;
    }

    public Mode GetMode()
    {
        string mode = new HttpProvider("mode").Provide().RootElement.ToString();

        return (Mode)ModeExtensions.FromJsonKey(mode, ModeMapChoice.StreamDict);
    }
    
    public VolumeSettings GetVolumeSetting(Device device, Mode mode, Channel channel)
    { 
        JsonDocument volumeSettings = new HttpProvider($"volumeSettings/{mode.ToJsonKey(ModeMapChoice.StreamerDict)}").Provide();
        JsonElement vSetting;
        
        if (device == Device.Master)
        {
            vSetting = volumeSettings.RootElement.GetProperty("masters").GetProperty(mode.ToJsonKey(ModeMapChoice.StreamDict));
        }
        else
        {
            vSetting = volumeSettings.RootElement.GetProperty("devices").GetProperty(device.ToJsonKey()).GetProperty(mode.ToJsonKey(ModeMapChoice.StreamDict));
        }
        
        if (mode == Mode.Streamer) vSetting = vSetting.GetProperty(channel.ToJsonKey());
        
        double volume = vSetting.GetProperty("volume").GetDouble();
        bool mute = vSetting.GetProperty("muted").GetBoolean();
        
        return new VolumeSettings(volume, mute);
    }
}