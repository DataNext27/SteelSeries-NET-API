using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Events;

public class SonarVolumeEvent : EventArgs
{
    // /volumeSettings/classic/game/Volume/0.27
    // /volumeSettings/streamer/monitoring/game/volume/0.99
    
    public double Volume { get; set; }
    
    public Mode Mode { get; set; }
    
    public Device Device { get; set; }
    public Channel? Channel { get; set; }
}