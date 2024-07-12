using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Interfaces;

public interface ISonarDataProvider
{
    /// <summary>
    /// Get the current mode used by Sonar
    /// </summary>
    /// <returns>A <see cref="Mode"/>, either Classic or Streamer</returns>
    Mode GetMode();

    VolumeSettings GetVolumeSetting(Device device, Mode mode, Channel channel);

    /// <summary>
    /// Get all audio configurations from Sonar
    /// </summary>
    /// <returns>An IEnumerable of <see cref="SonarAudioConfiguration"/></returns>
    IEnumerable<SonarAudioConfiguration> GetAllAudioConfigurations();

    /// <summary>
    /// Get all audio configurations of a <see cref="Device"/> from Sonar
    /// </summary>
    /// <param name="device">The device you want the configs</param>
    /// <returns>An IEnumerable of <see cref="SonarAudioConfiguration"/> ordered alphabetically</returns>
    IEnumerable<SonarAudioConfiguration> GetAudioConfigurations(Device device);

    /// <summary>
    /// Get the current audio configuration of a chosen <see cref="Device"/>
    /// </summary>
    /// <param name="device">The device you want the current config</param>
    /// <returns>A <see cref="SonarAudioConfiguration"/></returns>
    SonarAudioConfiguration GetSelectedAudioConfiguration(Device device);

    /// <summary>
    /// Get the associated device of a configuration
    /// </summary>
    /// <param name="configId">The id of the audio config</param>
    /// <returns>A <see cref="Device"/></returns>
    Device GetDeviceFromAudioConfigurationId(string configId);

    /// <summary>
    /// Get the actual ChatMix balance value
    /// </summary>
    /// <returns>A double between -1 and 1</returns>
    double GetChatMixBalance();

    /// <summary>
    /// Get the actual state of the ChatMix
    /// </summary>
    /// <returns>True if ChatMix is enabled <br/> False if ChatMix is disabled</returns>
    bool GetChatMixState();
    
    IEnumerable<RedirectionDevice> GetRedirectionDevices(Direction direction);
}