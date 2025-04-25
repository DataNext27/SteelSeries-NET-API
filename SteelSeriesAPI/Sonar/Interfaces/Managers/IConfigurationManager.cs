using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Interfaces.Managers;

public interface IConfigurationManager
{
    
    /// <summary>
    /// Get all audio configurations from Sonar
    /// </summary>
    /// <returns>An IEnumerable of <see cref="SonarAudioConfiguration"/></returns>
    IEnumerable<SonarAudioConfiguration> GetAllAudioConfigurations();
    
    /// <summary>
    /// Get all audio configurations of a <see cref="Channel"/> from Sonar
    /// </summary>
    /// <param name="channel">The channel you want the configs</param>
    /// <returns>An IEnumerable of <see cref="SonarAudioConfiguration"/> ordered alphabetically</returns>
    IEnumerable<SonarAudioConfiguration> GetAudioConfigurations(Channel channel);
    
    /// <summary>
    /// Get a specific audio configuration from Sonar
    /// </summary>
    /// <param name="configId">The id of the config</param>
    /// <returns>A <see cref="SonarAudioConfiguration"/></returns>
    SonarAudioConfiguration GetAudioConfiguration(string configId);
    
    /// <summary>
    /// Get the current audio configuration of a chosen <see cref="Channel"/>
    /// </summary>
    /// <param name="channel">The channel you want the current config</param>
    /// <returns>A <see cref="SonarAudioConfiguration"/></returns>
    SonarAudioConfiguration GetSelectedAudioConfiguration(Channel channel);
    
    /// <summary>
    /// Set the config of a Sonar <see cref="Channel"/> by giving its id
    /// </summary>
    /// <remarks>For more explanation, go on the GitHub wiki</remarks>
    /// <param name="configId">The id of the config</param>
    void SetConfig(string configId);
    
    /// <summary>
    /// Set the config of a Sonar <see cref="Channel"/> by giving a <see cref="SonarAudioConfiguration"/>
    /// </summary>
    /// <remarks>For more explanation, go on the GitHub wiki</remarks>
    /// <param name="config">The id <see cref="SonarAudioConfiguration"/></param>
    void SetConfig(SonarAudioConfiguration config);
    
    /// <summary>
    /// Set the config of a Sonar <see cref="Channel"/> by giving its name
    /// </summary>
    /// <remarks>For more explanation, go on the GitHub wiki</remarks>
    /// <param name="channel">The <see cref="Channel"/> you want to change the config</param>
    /// <param name="name">The name of the config</param>
    void SetConfigByName(Channel channel, string name);
}