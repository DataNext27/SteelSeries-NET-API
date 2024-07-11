namespace SteelSeriesAPI.Sonar.Models;

public class VolumeSettings
{
    public double Volume { get; set; }
    public bool Mute { get; set; }

    public VolumeSettings(double volume, bool mute)
    {
        this.Volume = volume;
        this.Mute = mute;
    }
}