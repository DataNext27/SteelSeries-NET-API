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
    /// Get the volume of a  Sonar <see cref="Device"/>
    /// </summary>
    /// <param name="device">The Sonar <see cref="Device"/> you want the volume</param>
    /// <returns>The volume of the channel in double, value between 0 and 1</returns>
    double GetVolume(Device device);
    
    /// <summary>
    /// Get the volume of a Steamer mode Sonar <see cref="Channel"/>
    /// </summary>
    /// <param name="device">The Sonar <see cref="Device"/> you want the volume</param>
    /// <param name="channel">The Sonar <see cref="Channel"/> you want the volume</param>
    /// <returns>The volume of the channel in double, value between 0 and 1</returns>
    double GetVolume(Device device, Channel channel);
    
    /// <summary>
    /// Get the mute state of a Sonar <see cref="Device"/>
    /// </summary>
    /// <param name="device">The Sonar <see cref="Device"/> you want the mute state</param>
    /// <returns>The mute state, a boolean</returns>
    bool GetMute(Device device);
    
    /// <summary>
    /// Get the mute state of a Streamer mode Sonar <see cref="Channel"/>
    /// </summary>
    /// <param name="device">The Sonar <see cref="Device"/> you want the mute state</param>
    /// <param name="channel">The Sonar <see cref="Channel"/> you want the mute state</param>
    /// <returns>The mute state, a boolean</returns>
    bool GetMute(Device device, Channel channel);
    
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
    /// Get all the in/output Redirection Devices (Windows devices)
    /// </summary>
    /// <param name="direction">The DataFlow of the device (In/Output)</param>
    /// <returns>A list of <see cref="RedirectionDevice"/></returns>
    IEnumerable<RedirectionDevice> GetRedirectionDevices(Direction direction);

    /// <summary>
    /// Get the current Redirection Device of a Sonar <see cref="Device"/>
    /// </summary>
    /// <param name="device">The Sonar <see cref="Device"/> you want the redirection device</param>
    /// <returns><see cref="RedirectionDevice"/></returns>
    RedirectionDevice GetClassicRedirectionDevice(Device device);

    /// <summary>
    /// Get the current Redirection Device of a Streamer mode Sonar <see cref="Channel"/>
    /// </summary>
    /// <param name="channel">The Sonar <see cref="Channel"/> you want the redirection device</param>
    /// <returns>A <see cref="RedirectionDevice"/></returns>
    RedirectionDevice GetStreamRedirectionDevice(Channel channel);
    
    /// <summary>
    /// Get the current Redirection Device of the Streamer mode Sonar <see cref="Device.Mic"/>
    /// </summary>
    /// <param name="device">The Sonar <see cref="Device"/> you want to change the redirection device</param>
    /// <returns><see cref="RedirectionDevice"/></returns>
    /// <remarks><paramref name="device"/> should be set to <see cref="Device.Mic"/> for it to work</remarks>
    RedirectionDevice GetStreamRedirectionDevice(Device device = Device.Mic);

    /// <summary>
    /// Get a Redirection Device using its ID
    /// </summary>
    /// <param name="deviceId">The ID of the Redirection Device</param>
    /// <returns><see cref="RedirectionDevice"/></returns>
    RedirectionDevice GetRedirectionDeviceFromId(string deviceId);

    /// <summary>
    /// Get the mute state of the Redirection of the chosen Sonar <see cref="Channel"/> of the chosen Sonar <see cref="Device"/>
    /// </summary>
    /// <param name="device">The Sonar <see cref="Device"/> you want the mute state of a Redirection channel</param>
    /// <param name="channel">The Sonar <see cref="Channel"/> you want to get the mute state</param>
    /// <returns>The current state, un/muted</returns>
    bool GetRedirectionState(Device device, Channel channel);

    /// <summary>
    /// Get the current state of the Audience Monitoring
    /// </summary>
    /// <returns>The current state, un/muted</returns>
    bool GetAudienceMonitoringState();
    
    /// <summary>
    /// Get the apps which their audio is redirected to a Sonar <see cref="Device"/>
    /// </summary>
    /// <param name="device">The Sonar <see cref="Device"/> you want the associated processes</param>
    /// <returns>A list of <see cref="RoutedProcess"/></returns>
    IEnumerable<RoutedProcess> GetRoutedProcess(Device device);
}