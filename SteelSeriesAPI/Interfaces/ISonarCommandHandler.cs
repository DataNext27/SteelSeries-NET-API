using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Interfaces;

public interface ISonarCommandHandler
{
    void SetMode(Mode mode);

    void SetVolume(double vol, Device device, Mode mode, Channel channel);

    void SetMute(bool mute, Device device, Mode mode, Channel channel);

    void SetConfig(string configId);
    
    void SetConfig(Device device, string name);
}