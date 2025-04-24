using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Interfaces;

public interface ISonarDataProvider
{
    /// <summary>
    /// Get the current mode used by Sonar
    /// </summary>
    /// <returns>A <see cref="Mode"/>, either Classic or Streamer</returns>
    Mode GetMode();

    /// <summary>
    /// Get the volume of a  Sonar <see cref="Channel"/>
    /// </summary>
    /// <param name="channel">The Sonar <see cref="Channel"/> you want the volume</param>
    /// <returns>The volume of the mix in double, value between 0 and 1</returns>
    double GetVolume(Channel channel);
    
    /// <summary>
    /// Get the volume of a Steamer mode Sonar <see cref="Mix"/>
    /// </summary>
    /// <param name="channel">The Sonar <see cref="Channel"/> you want the volume</param>
    /// <param name="mix">The Sonar <see cref="Mix"/> you want the volume</param>
    /// <returns>The volume of the mix in double, value between 0 and 1</returns>
    double GetVolume(Channel channel, Mix mix);
    
    /// <summary>
    /// Get the mute state of a Sonar <see cref="Channel"/>
    /// </summary>
    /// <param name="channel">The Sonar <see cref="Channel"/> you want the mute state</param>
    /// <returns>The mute state, a boolean</returns>
    bool GetMute(Channel channel);
    
    /// <summary>
    /// Get the mute state of a Streamer mode Sonar <see cref="Mix"/>
    /// </summary>
    /// <param name="channel">The Sonar <see cref="Channel"/> you want the mute state</param>
    /// <param name="mix">The Sonar <see cref="Mix"/> you want the mute state</param>
    /// <returns>The mute state, a boolean</returns>
    bool GetMute(Channel channel, Mix mix);
    
    /// <summary>
    /// Get all audio configurations from Sonar
    /// </summary>
    /// <returns>An IEnumerable of <see cref="SonarAudioConfiguration"/></returns>
    IEnumerable<SonarAudioConfiguration> GetAllAudioConfigurations();

    /// <summary>
    /// Get a specific audio configuration from Sonar
    /// </summary>
    /// <param name="configId">The id of the config</param>
    /// <returns>A <see cref="SonarAudioConfiguration"/></returns>
    SonarAudioConfiguration GetAudioConfiguration(string configId);

    /// <summary>
    /// Get all audio configurations of a <see cref="Channel"/> from Sonar
    /// </summary>
    /// <param name="channel">The channel you want the configs</param>
    /// <returns>An IEnumerable of <see cref="SonarAudioConfiguration"/> ordered alphabetically</returns>
    IEnumerable<SonarAudioConfiguration> GetAudioConfigurations(Channel channel);

    /// <summary>
    /// Get the current audio configuration of a chosen <see cref="Channel"/>
    /// </summary>
    /// <param name="channel">The channel you want the current config</param>
    /// <returns>A <see cref="SonarAudioConfiguration"/></returns>
    SonarAudioConfiguration GetSelectedAudioConfiguration(Channel channel);

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
    /// <param name="dataFlow">The DataFlow of the channel (In/Output)</param>
    /// <returns>A list of <see cref="PlaybackDevice"/></returns>
    IEnumerable<PlaybackDevice> GetPlaybackDevices(DataFlow dataFlow);

    /// <summary>
    /// Get the current Redirection Channel of a Sonar <see cref="Channel"/>
    /// </summary>
    /// <param name="channel">The Sonar <see cref="Channel"/> you want the redirection channel</param>
    /// <returns><see cref="PlaybackDevice"/></returns>
    PlaybackDevice GetClassicPlaybackDevice(Channel channel);

    /// <summary>
    /// Get the current Redirection Channel of a Streamer mode Sonar <see cref="Mix"/>
    /// </summary>
    /// <param name="mix">The Sonar <see cref="Mix"/> you want the redirection channel</param>
    /// <returns>A <see cref="PlaybackDevice"/></returns>
    PlaybackDevice GetStreamPlaybackDevice(Mix mix);
    
    /// <summary>
    /// Get the current Redirection Channel of the Streamer mode Sonar <see cref="Channel.MIC"/>
    /// </summary>
    /// <param name="channel">The Sonar <see cref="Channel"/> you want to change the redirection channel</param>
    /// <returns><see cref="PlaybackDevice"/></returns>
    /// <remarks><paramref name="channel"/> should be set to <see cref="Channel.MIC"/> for it to work</remarks>
    PlaybackDevice GetStreamPlaybackDevice(Channel channel = Channel.MIC);

    /// <summary>
    /// Get a Redirection Channel using its ID
    /// </summary>
    /// <param name="deviceId">The ID of the Redirection Channel</param>
    /// <returns><see cref="PlaybackDevice"/></returns>
    PlaybackDevice GetPlaybackDeviceFromId(string deviceId);

    /// <summary>
    /// Get the mute state of the Redirection of the chosen Sonar <see cref="Mix"/> of the chosen Sonar <see cref="Channel"/>
    /// </summary>
    /// <param name="channel">The Sonar <see cref="Channel"/> you want the mute state of a Redirection mix</param>
    /// <param name="mix">The Sonar <see cref="Mix"/> you want to get the mute state</param>
    /// <returns>The current state, un/muted</returns>
    bool GetRedirectionState(Channel channel, Mix mix);

    /// <summary>
    /// Get the current state of the Audience Monitoring
    /// </summary>
    /// <returns>The current state, un/muted</returns>
    bool GetAudienceMonitoringState();
    
    /// <summary>
    /// Get the apps which their audio is redirected to a Sonar <see cref="Channel"/>
    /// </summary>
    /// <param name="channel">The Sonar <see cref="Channel"/> you want the associated processes</param>
    /// <returns>A list of <see cref="RoutedProcess"/></returns>
    IEnumerable<RoutedProcess> GetRoutedProcess(Channel channel);
}