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

    /// <summary>
    /// Get the volume settings of a device/channel
    /// </summary>
    /// <param name="device">The <see cref="Device"/> you want the audio settings</param>
    /// <param name="mode">The <see cref="Mode"/> you want the audio settings</param>
    /// <param name="channel">The <see cref="Channel"/> you want the audio settings</param>
    /// <returns><see cref="VolumeSettings"/></returns>
    /// <remarks>Prefer using <see cref="SonarBridge.GetVolume"/> or <see cref="SonarBridge.GetMute"/> instead <br/>
    /// To use <paramref name="channel"/>, you should put <paramref name="mode"/> to <see cref="Mode.Streamer"/></remarks>
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

    /// <summary>
    /// Get all the in/output redirection devices (Windows devices)
    /// </summary>
    /// <param name="direction">The DataFlow of the device (In/Output)</param>
    /// <returns>A list of <see cref="RedirectionDevice"/></returns>
    IEnumerable<RedirectionDevice> GetRedirectionDevices(Direction direction);

    /// <summary>
    /// Get the current classic redirection device of a <see cref="Device"/>
    /// </summary>
    /// <param name="device">The <see cref="Device"/> you want the redirection device</param>
    /// <returns>A <see cref="RedirectionDevice"/></returns>
    RedirectionDevice GetClassicRedirectionDevice(Device device);

    /// <summary>
    /// Get the current stream redirection device of a <see cref="Channel"/>
    /// </summary>
    /// <param name="channel">The <see cref="Channel"/> you want the redirection device</param>
    /// <returns><see cref="RedirectionDevice"/></returns>
    RedirectionDevice GetStreamRedirectionDevice(Channel channel);
    
    /// <summary>
    /// Get the current stream redirection device of the <see cref="Device.Mic"/> <see cref="Device"/>
    /// </summary>
    /// <param name="device">The <see cref="Device"/> you want to change the redirection device</param>
    /// <returns><see cref="RedirectionDevice"/></returns>
    /// <remarks><paramref name="device"/> should be set to <see cref="Device.Mic"/> for it to work</remarks>
    RedirectionDevice GetStreamRedirectionDevice(Device device = Device.Mic);

    /// <summary>
    /// Get a redirection device using its ID
    /// </summary>
    /// <param name="deviceId">The ID of the redirection device</param>
    /// <returns><see cref="RedirectionDevice"/></returns>
    RedirectionDevice GetRedirectionDeviceFromId(string deviceId);

    /// <summary>
    /// Get the current muted state of the redirection of the chosen <see cref="Channel"/> of the chosen <see cref="Device"/>
    /// </summary>
    /// <param name="device">The <see cref="Device"/> you want to get the muted state of a redirection channel</param>
    /// <param name="channel">The <see cref="Channel"/> you want to get the muted state</param>
    /// <returns>The current state, un/muted</returns>
    bool GetRedirectionState(Device device, Channel channel);

    /// <summary>
    /// Get the current state of the audience monitoring
    /// </summary>
    /// <returns>The current state, un/muted</returns>
    bool GetAudienceMonitoringState();
    
    /// <summary>
    /// Get the apps which their audio is redirected to a <see cref="Device"/>
    /// </summary>
    /// <param name="device">The <see cref="Device"/> you want the associated processes</param>
    /// <returns>A list of <see cref="RoutedProcess"/></returns>
    IEnumerable<RoutedProcess> GetRoutedProcess(Device device);
}