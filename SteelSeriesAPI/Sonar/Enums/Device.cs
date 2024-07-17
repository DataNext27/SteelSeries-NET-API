namespace SteelSeriesAPI.Sonar.Enums;

public enum Device
{
    Master,
    Game,
    Chat,
    Media,
    Aux,
    Mic
}

public enum DeviceMapChoice
{
    JsonDict,
    HttpDict,
    DeviceDict
}

public static class DeviceExtensions
{
    private static readonly Dictionary<Device, string> DeviceJsonMap = new Dictionary<Device, string>
    {
        { Device.Master, "masters" },
        { Device.Game, "game" },
        { Device.Chat, "chatRender" }, 
        { Device.Media, "media" },
        { Device.Aux, "aux" },
        { Device.Mic, "chatCapture" } 
    };
    
    private static readonly Dictionary<Device, string> DeviceHttpMap = new Dictionary<Device, string>
    {
        { Device.Master, "Master" },
        { Device.Game, "game" },
        { Device.Chat, "chatRender" }, 
        { Device.Media, "media" },
        { Device.Aux, "aux" },
        { Device.Mic, "chatCapture" } 
    };

    private static readonly Dictionary<Device, string> DeviceMap = new Dictionary<Device, string>
    {
        { Device.Master, "master" },
        { Device.Game, "game" },
        { Device.Chat, "chat" }, 
        { Device.Media, "media" },
        { Device.Aux, "aux" },
        { Device.Mic, "mic" } 
    };
    
    public static string ToDictKey(this Device device, DeviceMapChoice context = DeviceMapChoice.JsonDict)
    {
        return context switch
        {
            DeviceMapChoice.JsonDict => DeviceJsonMap.ContainsKey(device) ? DeviceJsonMap[device] : null,
            DeviceMapChoice.HttpDict => DeviceHttpMap.ContainsKey(device) ? DeviceHttpMap[device] : null,
            DeviceMapChoice.DeviceDict => DeviceMap.ContainsKey(device) ? DeviceMap[device] : null,
            _ => null
        };
    }
    
    public static Device? FromDictKey(string jsonKey, DeviceMapChoice context = DeviceMapChoice.JsonDict)
    {
        var map = context switch
        {
            DeviceMapChoice.JsonDict => DeviceJsonMap,
            DeviceMapChoice.HttpDict => DeviceHttpMap,
            DeviceMapChoice.DeviceDict => DeviceMap,
            _ => null
        };

        if (map != null)
        {
            foreach (var pair in map)
            {
                if (pair.Value == jsonKey)
                {
                    return pair.Key;
                }
            }
        }

        return null;
    }
}