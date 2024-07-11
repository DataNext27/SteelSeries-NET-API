using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Interfaces;

public interface ISonarDataProvider
{
    Mode GetMode();

    VolumeSettings GetVolumeSetting(Device device, Mode mode, Channel channel);

    /// <summary>
    /// Get all audio configurations from Sonar
    /// </summary>
    /// <returns>An IEnumerable of SonarAudioConfigs</returns>
    IEnumerable<SonarAudioConfiguration> GetAllAudioConfigurations();

    /// <summary>
    /// Get all audio configurations of a device from Sonar
    /// </summary>
    /// <param name="device">The device you want the configs</param>
    /// <returns>An IEnumerable of SonarAudioConfigs ordered alphabetically</returns>
    IEnumerable<SonarAudioConfiguration> GetAudioConfigurations(Device device);

    SonarAudioConfiguration GetSelectedAudioConfiguration(Device device);

    Device GetDeviceFromAudioConfigurationId(string configId);
}