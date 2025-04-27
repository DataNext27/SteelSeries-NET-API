using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sonar.Interfaces.Managers;

/// <summary>
/// Manage the volumes and muted state of each <see cref="Channel"/>
/// </summary>
public interface IVolumeSettingsManager
{
    /// <summary>
    /// Get the volume of a Sonar <see cref="Channel"/>
    /// </summary>
    /// <param name="channel">The Sonar <see cref="Channel"/> you want the volume of</param>
    /// <returns>The volume of the channel in double, value between 0 and 1</returns>
    double GetVolume(Channel channel);
    
    /// <summary>
    /// Get the volume of a Steamer mode Sonar <see cref="Mix"/>
    /// </summary>
    /// <param name="channel">The Sonar <see cref="Channel"/> of the <see cref="Mix"/> you want the volume of</param>
    /// <param name="mix">The Sonar <see cref="Mix"/> you want the volume of</param>
    /// <returns>The volume of the mix in double, value between 0 and 1</returns>
    double GetVolume(Channel channel, Mix mix);
    
    /// <summary>
    /// Get the mute state of a Sonar <see cref="Channel"/>
    /// </summary>
    /// <param name="channel">The Sonar <see cref="Channel"/> you want the mute state of</param>
    /// <returns>The mute state, a boolean</returns>
    bool GetMute(Channel channel);
    
    /// <summary>
    /// Get the mute state of a Streamer mode Sonar <see cref="Mix"/>
    /// </summary>
    /// <param name="channel">The Sonar <see cref="Channel"/> of the <see cref="Mix"/> you want the mute state of</param>
    /// <param name="mix">The Sonar <see cref="Mix"/> you want the mute state of</param>
    /// <returns>The mute state, a boolean</returns>
    bool GetMute(Channel channel, Mix mix);
    
    /// <summary>
    /// Set the volume of a Sonar <see cref="Channel"/>
    /// </summary>
    /// <param name="vol">The volume you want to set, between 1 and 0</param>
    /// <param name="channel">The <see cref="Channel"/> you want to change the volume of</param>
    void SetVolume(double vol, Channel channel);
    
    /// <summary>
    /// Set the volume of a Streamer mode Sonar <see cref="Mix"/>
    /// </summary>
    /// <param name="vol">The volume you want to set, between 1 and 0</param>
    /// <param name="channel">The <see cref="Channel"/> of the <see cref="Mix"/> you want to change the volume of</param>
    /// <param name="mix">The <see cref="Mix"/> you want to change the volume of</param>
    void SetVolume(double vol, Channel channel, Mix mix);
    
    /// <summary>
    /// Mute or unmute a Sonar <see cref="Channel"/>
    /// </summary>
    /// <param name="mute">The new muted state</param>
    /// <param name="channel">The <see cref="Channel"/> you want to un/mute</param>
    void SetMute(bool mute, Channel channel);
    
    /// <summary>
    /// Mute or unmute a Streamer mode Sonar <see cref="Mix"/>
    /// </summary>
    /// <param name="mute">The new muted state</param>
    /// <param name="channel">The <see cref="Channel"/> of the <see cref="Mix"/> you want to un/mute</param>
    /// <param name="mix">The <see cref="Mix"/> you want to un/mute</param>
    void SetMute(bool mute, Channel channel, Mix mix);
}