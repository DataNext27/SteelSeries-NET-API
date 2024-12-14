using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Interfaces;

public interface ISonarCommandHandler
{
    /// <summary>
    /// Set the <see cref="Mode"/> Sonar will be using
    /// </summary>
    /// <param name="mode">The <see cref="Mode"/> you want to set</param>
    void SetMode(Mode mode);

    /// <summary>
    /// Set the volume of a Sonar <see cref="Device"/>
    /// </summary>
    /// <param name="vol">The volume you want to set, between 1 and 0</param>
    /// <param name="device">The <see cref="Device"/> you want to change the volume</param>
    void SetVolume(double vol, Device device);
    
    /// <summary>
    /// Set the volume of a Streamer mode Sonar <see cref="Device"/>
    /// </summary>
    /// <param name="vol">The volume you want to set, between 1 and 0</param>
    /// <param name="device">The <see cref="Device"/> you want to change the volume</param>
    /// <param name="channel">The <see cref="Channel"/> you want to change the volume</param>
    void SetVolume(double vol, Device device, Channel channel);

    /// <summary>
    /// Mute or unmute a Sonar <see cref="Device"/>
    /// </summary>
    /// <param name="mute"></param>
    /// <param name="device">The <see cref="Device"/> you want to un/mute</param>
    /// <param name="mode">The <see cref="Mode"/> in which you want to un/mute</param>
    /// <param name="channel">The <see cref="Channel"/> you want to un/mute</param>
    void SetMute(bool mute, Device device, Mode mode, Channel channel);

    /// <summary>
    /// Set the config of a Sonar <see cref="Device"/> by giving its id
    /// </summary>
    /// <remarks>For more explanation, go on the GitHub wiki</remarks>
    /// <param name="configId">The id of the config</param>
    void SetConfig(string configId);
    
    /// <summary>
    /// Set the config of a Sonar <see cref="Device"/> by giving its name
    /// </summary>
    /// <remarks>For more explanation, go on the GitHub wiki</remarks>
    /// <param name="device">The <see cref="Device"/> you want to change the config</param>
    /// <param name="name">The name of the config</param>
    void SetConfig(Device device, string name);
    
    /// <summary>
    /// Set the balance of the ChatMix
    /// </summary>
    /// <remarks>-1 to balance to Game device<br/>1 to balance to Chat device</remarks>
    /// <param name="balance">A <see cref="double"/> between -1 and 0</param>
    void SetChatMixBalance(double balance);

    /// <summary>
    /// Set the classic mode redirection device of a <see cref="Device"/> using its ID
    /// </summary>
    /// <param name="deviceId">The id of the new redirection device</param>
    /// <param name="device">The <see cref="Device"/> you want to change the redirection device</param>
    void SetClassicRedirectionDevice(string deviceId, Device device);

    /// <summary>
    /// Set the streamer mode redirection device of a <see cref="Channel"/> using its ID
    /// </summary>
    /// <param name="deviceId">The id of the new redirection device</param>
    /// <param name="channel">The <see cref="Channel"/> you want to change the redirection device</param>
    void SetStreamRedirectionDevice(string deviceId, Channel channel);

    /// <summary>
    /// Set the streamer mode redirection device of the mic using its ID
    /// </summary>
    /// <param name="deviceId">The id of the new redirection device</param>
    /// <param name="device">The <see cref="Device"/> you want to change the redirection device</param>
    /// <remarks><paramref name="device"/> should be set to <see cref="Device.Mic"/> for it to work</remarks>
    void SetStreamRedirectionDevice(string deviceId, Device device = Device.Mic);

    /// <summary>
    /// Mute or unmute the redirection of the chosen <see cref="Channel"/> of the chosen <see cref="Device"/>
    /// </summary>
    /// <param name="newState">The new state of the redirection</param>
    /// <param name="device">The <see cref="Device"/> you want to un/mute a redirection channel</param>
    /// <param name="channel">The <see cref="Channel"/> you want to un/mute</param>
    void SetRedirectionState(bool newState, Device device, Channel channel);

    /// <summary>
    /// Listen to what your audience hear
    /// </summary>
    /// <param name="newState">The new state, un/muted</param>
    void SetAudienceMonitoringState(bool newState);

    /// <summary>
    /// Redirect the audio of an app to a <see cref="Device"/>
    /// </summary>
    /// <param name="pId">The process ID of the app</param>
    /// <param name="device">The <see cref="Device"/> you want to set the app audio</param>
    void SetProcessToDeviceRouting(int pId, Device device);
}