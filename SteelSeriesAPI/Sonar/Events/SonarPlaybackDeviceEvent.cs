using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sonar.Events;

public class SonarPlaybackDeviceEvent : EventArgs
{
    // /classicRedirections/game/deviceId/%7B0.0.0.00000000%7D.%7B1e1ebefc-2c51-4675-aebe-085a06efd255%7D
    // /streamRedirections/monitoring/deviceId/%7B0.0.0.00000000%7D.%7B1e1ebefc-2c51-4675-aebe-085a06efd255%7D
    
    public string PlaybackDeviceId { get; set; }
    
    public Mode Mode { get; set; }
    
    public Channel? Channel { get; set; }
    
    public Mix? Mix { get; set; }
}