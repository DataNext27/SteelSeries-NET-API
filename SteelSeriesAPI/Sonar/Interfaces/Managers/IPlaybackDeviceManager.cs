using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Interfaces.Managers;

/// <summary>
/// Manage the playback device of each <see cref="Channel"/>
/// </summary>
public interface IPlaybackDeviceManager
{
    
    
    /// <summary>
    /// Get all the Playback Devices (Windows devices)
    /// </summary>
    /// <returns>A list of <see cref="PlaybackDevice"/></returns>
    IEnumerable<PlaybackDevice> GetAllPlaybackDevices();
    
    /// <summary>
    /// Get all output playback devices
    /// </summary>
    /// <returns>A list of <see cref="PlaybackDevice"/></returns>
    IEnumerable<PlaybackDevice> GetOutputPlaybackDevices();
    
    /// <summary>
    /// Get all input playback devices
    /// </summary>
    /// <returns>A list of <see cref="PlaybackDevice"/></returns>
    IEnumerable<PlaybackDevice> GetInputPlaybackDevices();
    
    /// <summary>
    /// Get the playback device of a <see cref="Channel"/>
    /// </summary>
    /// <param name="channel"><see cref="Channel"/></param>
    /// <returns><see cref="PlaybackDevice"/></returns>
    PlaybackDevice GetPlaybackDevice(Channel channel);
    
    /// <summary>
    /// Get the playback device of a <see cref="Channel"/> depending on the mode<br/>
    /// Mainly used to get the Streamer Mode Mic
    /// </summary>
    /// <param name="channel"><see cref="Channel"/></param>
    /// <param name="mode"><see cref="Mode"/></param>
    /// <returns><see cref="PlaybackDevice"/></returns>
    PlaybackDevice GetPlaybackDevice(Channel channel, Mode mode);
    
    /// <summary>
    /// Get the playback device of a <see cref="Mix"/>
    /// </summary>
    /// <param name="mix"><see cref="Mix"/></param>
    /// <returns><see cref="PlaybackDevice"/></returns>
    PlaybackDevice GetPlaybackDevice(Mix mix);
    
    /// <summary>
    /// Get a playback device using its id
    /// </summary>
    /// <param name="deviceId">The id of the device</param>
    /// <returns><see cref="PlaybackDevice"/></returns>
    PlaybackDevice GetPlaybackDevice(string deviceId);
    
    /// <summary>
    /// Set the playback device of a <see cref="Channel"/>
    /// </summary>
    /// <param name="deviceId">The id of the device</param>
    /// <param name="channel"><see cref="Channel"/></param>
    void SetPlaybackDevice(string deviceId, Channel channel);
    
    /// <summary>
    /// Set the playback device of a <see cref="Channel"/> depending on the mode<br/>
    /// Mainly used to change the playback device of the Streamer Mode <see cref="Channel.MIC"/>
    /// </summary>
    /// <param name="deviceId">The id of the device</param>
    /// <param name="channel"><see cref="Channel"/></param>
    /// <param name="mode"><see cref="Mode"/></param>
    void SetPlaybackDevice(string deviceId, Channel channel, Mode mode);
    
    /// <summary>
    /// Set the playback device of a <see cref="Mix"/>
    /// </summary>
    /// <param name="deviceId">The id of the device</param>
    /// <param name="mix"><see cref="Mix"/></param>
    void SetPlaybackDevice(string deviceId, Mix mix);
    
    /// <summary>
    /// Set the playback device of a <see cref="Channel"/>
    /// </summary>
    /// <param name="device"><see cref="PlaybackDevice"/></param>
    /// <param name="channel"><see cref="Channel"/></param>
    void SetPlaybackDevice(PlaybackDevice device, Channel channel);
    
    /// <summary>
    /// Set the playback device of a <see cref="Channel"/> depending on the mode<br/>
    /// Mainly used to change the playback device of the Streamer Mode <see cref="Channel.MIC"/>
    /// </summary>
    /// <param name="device"><see cref="PlaybackDevice"/></param>
    /// <param name="channel"><see cref="Channel"/></param>
    /// <param name="mode"><see cref="Mode"/></param>
    void SetPlaybackDevice(PlaybackDevice device, Channel channel, Mode mode);
    
    /// <summary>
    /// Set the playback device of a <see cref="Mix"/>
    /// </summary>
    /// <param name="device"><see cref="PlaybackDevice"/></param>
    /// <param name="mix"><see cref="Mix"/></param>
    void SetPlaybackDevice(PlaybackDevice device, Mix mix);
}