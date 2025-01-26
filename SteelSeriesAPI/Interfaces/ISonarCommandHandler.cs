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
    /// Set the volume of a Streamer mode Sonar <see cref="Channel"/>
    /// </summary>
    /// <param name="vol">The volume you want to set, between 1 and 0</param>
    /// <param name="device">The <see cref="Device"/> you want to change the volume</param>
    /// <param name="channel">The <see cref="Channel"/> you want to change the volume</param>
    void SetVolume(double vol, Device device, Channel channel);

    /// <summary>
    /// Mute or unmute a Sonar <see cref="Device"/>
    /// </summary>
    /// <param name="mute">The new muted state</param>
    /// <param name="device">The <see cref="Device"/> you want to un/mute</param>
    void SetMute(bool mute, Device device);

    /// <summary>
    /// Mute or unmute a Streamer mode Sonar <see cref="Channel"/>
    /// </summary>
    /// <param name="mute">The new muted state</param>
    /// <param name="device">The <see cref="Device"/> you want to un/mute</param>
    /// <param name="channel">The <see cref="Channel"/> you want to un/mute</param>
    void SetMute(bool mute, Device device, Channel channel);

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
    /// <param name="balance">A <see cref="double"/> between -1 and 1</param>
    void SetChatMixBalance(double balance);

    /// <summary>
    /// Set the Redirection Device of a Sonar <see cref="Device"/> using its ID
    /// </summary>
    /// <param name="deviceId">The id of the new Redirection Device</param>
    /// <param name="device">The Sonar <see cref="Device"/> you want to change the Redirection Device</param>
    void SetClassicRedirectionDevice(string deviceId, Device device);

    /// <summary>
    /// Set the Redirection Device of a Streamer mode Sonar <see cref="Channel"/> using its ID
    /// </summary>
    /// <param name="deviceId">The id of the new Redirection Device</param>
    /// <param name="channel">The Sonar <see cref="Channel"/> you want to change the Redirection Device</param>
    void SetStreamRedirectionDevice(string deviceId, Channel channel);

    /// <summary>
    /// Set the Redirection Device of the Streamer mode Sonar <see cref="Device.Mic"/> using its ID
    /// </summary>
    /// <param name="deviceId">The id of the new Redirection Device</param>
    /// <param name="device">The Sonar <see cref="Device"/> you want to change the redirection device</param>
    /// <remarks><paramref name="device"/> should be set to <see cref="Device.Mic"/> for it to work</remarks>
    void SetStreamRedirectionDevice(string deviceId, Device device = Device.Mic);

    /// <summary>
    /// Enable or disable the Redirection of the chosen Sonar <see cref="Channel"/> of the chosen Sonar <see cref="Device"/>
    /// </summary>
    /// <param name="newState">The new state of the Redirection</param>
    /// <param name="device">The Sonar <see cref="Device"/> you want to un/mute a Sonar redirection channel</param>
    /// <param name="channel">The Sonar <see cref="Channel"/> you want to un/mute</param>
    void SetRedirectionState(bool newState, Device device, Channel channel);

    /// <summary>
    /// Listen to what your audience hear
    /// </summary>
    /// <param name="newState">The new state, un/muted</param>
    void SetAudienceMonitoringState(bool newState);

    /// <summary>
    /// Redirect the audio of an app to a Sonar <see cref="Device"/>
    /// </summary>
    /// <param name="pId">The process ID of the app</param>
    /// <param name="device">The Sonar <see cref="Device"/> you want to set the app audio</param>
    void SetProcessToDeviceRouting(int pId, Device device);
}