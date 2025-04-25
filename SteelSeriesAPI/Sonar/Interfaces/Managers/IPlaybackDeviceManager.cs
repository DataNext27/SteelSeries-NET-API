using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sonar.Interfaces.Managers;

public interface IPlaybackDeviceManager
{
    /// <summary>
    /// Get all the in/output Redirection Devices (Windows devices)
    /// </summary>
    /// <param name="dataFlow">The DataFlow of the channel (In/Output)</param>
    /// <returns>A list of <see cref="PlaybackDevice"/></returns>
    IEnumerable<PlaybackDevice> GetPlaybackDevices(DataFlow _dataFlow);
    
    /// <summary>
    /// Get a Redirection Channel using its ID
    /// </summary>
    /// <param name="deviceId">The ID of the Redirection Channel</param>
    /// <returns><see cref="PlaybackDevice"/></returns>
    PlaybackDevice GetPlaybackDevice(string deviceId);
    
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
    PlaybackDevice GetStreamerPlaybackDevice(Mix mix);
    
    /// <summary>
    /// Get the current Redirection Channel of the Streamer mode Sonar <see cref="Channel.MIC"/>
    /// </summary>
    /// <param name="channel">The Sonar <see cref="Channel"/> you want to change the redirection channel</param>
    /// <returns><see cref="PlaybackDevice"/></returns>
    /// <remarks><paramref name="channel"/> should be set to <see cref="Channel.MIC"/> for it to work</remarks>
    PlaybackDevice GetStreamerPlaybackDevice(Channel channel = Channel.MIC);
    
    /// <summary>
    /// Set the Redirection Channel of a Sonar <see cref="Channel"/> using its ID
    /// </summary>
    /// <param name="deviceId">The id of the new Redirection Channel</param>
    /// <param name="channel">The Sonar <see cref="Channel"/> you want to change the Redirection Channel</param>
    void SetClassicPlaybackDevice(string deviceId, Channel channel);
    
    /// <summary>
    /// Set the Redirection Channel of a Streamer mode Sonar <see cref="Mix"/> using its ID
    /// </summary>
    /// <param name="deviceId">The id of the new Redirection Channel</param>
    /// <param name="mix">The Sonar <see cref="Mix"/> you want to change the Redirection Channel</param>
    void SetStreamerPlaybackDevice(string deviceId, Mix mix);
    
    /// <summary>
    /// Set the Redirection Channel of the Streamer mode Sonar <see cref="Channel.MIC"/> using its ID
    /// </summary>
    /// <param name="deviceId">The id of the new Redirection Channel</param>
    /// <param name="channel">The Sonar <see cref="Channel"/> you want to change the redirection channel</param>
    /// <remarks><paramref name="channel"/> should be set to <see cref="Channel.MIC"/> for it to work</remarks>
    void SetStreamerPlaybackDevice(string deviceId, Channel channel = Channel.MIC);
    void SetClassicPlaybackDevice(PlaybackDevice playbackDevice, Channel channel);
    void SetStreamerPlaybackDevice(PlaybackDevice playbackDevice, Mix mix);
    void SetStreamerPlaybackDevice(PlaybackDevice playbackDevice, Channel channel = Channel.MIC);
}