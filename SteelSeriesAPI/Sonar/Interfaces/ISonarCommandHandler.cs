using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sonar.Interfaces;

public interface ISonarCommandHandler
{
    /// <summary>
    /// Set the <see cref="Mode"/> Sonar will be using
    /// </summary>
    /// <param name="mode">The <see cref="Mode"/> you want to set</param>
    void SetMode(Mode mode);

    /// <summary>
    /// Set the volume of a Sonar <see cref="Channel"/>
    /// </summary>
    /// <param name="vol">The volume you want to set, between 1 and 0</param>
    /// <param name="channel">The <see cref="Channel"/> you want to change the volume</param>
    void SetVolume(double vol, Channel channel);
    
    /// <summary>
    /// Set the volume of a Streamer mode Sonar <see cref="Mix"/>
    /// </summary>
    /// <param name="vol">The volume you want to set, between 1 and 0</param>
    /// <param name="channel">The <see cref="Channel"/> you want to change the volume</param>
    /// <param name="mix">The <see cref="Mix"/> you want to change the volume</param>
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
    /// <param name="channel">The <see cref="Channel"/> you want to un/mute</param>
    /// <param name="mix">The <see cref="Mix"/> you want to un/mute</param>
    void SetMute(bool mute, Channel channel, Mix mix);

    /// <summary>
    /// Set the config of a Sonar <see cref="Channel"/> by giving its id
    /// </summary>
    /// <remarks>For more explanation, go on the GitHub wiki</remarks>
    /// <param name="configId">The id of the config</param>
    void SetConfig(string configId);
    
    /// <summary>
    /// Set the config of a Sonar <see cref="Channel"/> by giving its name
    /// </summary>
    /// <remarks>For more explanation, go on the GitHub wiki</remarks>
    /// <param name="channel">The <see cref="Channel"/> you want to change the config</param>
    /// <param name="name">The name of the config</param>
    void SetConfig(Channel channel, string name);
    
    /// <summary>
    /// Set the balance of the ChatMix
    /// </summary>
    /// <remarks>-1 to balance to Game channel<br/>1 to balance to Chat channel</remarks>
    /// <param name="balance">A <see cref="double"/> between -1 and 1</param>
    void SetChatMixBalance(double balance);

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
    void SetStreamPlaybackDevice(string deviceId, Mix mix);

    /// <summary>
    /// Set the Redirection Channel of the Streamer mode Sonar <see cref="Channel.MIC"/> using its ID
    /// </summary>
    /// <param name="deviceId">The id of the new Redirection Channel</param>
    /// <param name="channel">The Sonar <see cref="Channel"/> you want to change the redirection channel</param>
    /// <remarks><paramref name="channel"/> should be set to <see cref="Channel.MIC"/> for it to work</remarks>
    void SetStreamPlaybackDevice(string deviceId, Channel channel = Channel.MIC);

    /// <summary>
    /// Enable or disable the Redirection of the chosen Sonar <see cref="Mix"/> of the chosen Sonar <see cref="Channel"/>
    /// </summary>
    /// <param name="newState">The new state of the Redirection</param>
    /// <param name="channel">The Sonar <see cref="Channel"/> you want to un/mute a Sonar redirection mix</param>
    /// <param name="mix">The Sonar <see cref="Mix"/> you want to un/mute</param>
    void SetRedirectionState(bool newState, Channel channel, Mix mix);

    /// <summary>
    /// Listen to what your audience hear
    /// </summary>
    /// <param name="newState">The new state, un/muted</param>
    void SetAudienceMonitoringState(bool newState);

    /// <summary>
    /// Redirect the audio of an app to a Sonar <see cref="Channel"/>
    /// </summary>
    /// <param name="pId">The process ID of the app</param>
    /// <param name="channel">The Sonar <see cref="Channel"/> you want to set the app audio</param>
    void SetProcessToDeviceRouting(int pId, Channel channel);
}