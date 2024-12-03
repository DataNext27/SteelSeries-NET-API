using System.Threading.Channels;
using SteelSeriesAPI.Sonar.Enums;
using Channel = SteelSeriesAPI.Sonar.Enums.Channel;

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
    /// <param name="mode">The <see cref="Mode"/> in which you want to change the volume</param>
    /// <param name="channel">The <see cref="Sonar.Enums.Channel"/> you want to change the volume</param>
    void SetVolume(double vol, Device device, Mode mode, Channel channel);

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

    void SetClassicRedirectionDevice(string deviceId, Device device);

    void SetStreamRedirectionDevice(string deviceId, Channel channel);

    void SetStreamRedirectionDevice(string deviceId, Device device);

    void SetRedirectionState(bool newState, Device device, Channel channel);

    void SetAudienceMonitoringState(bool newState);

    void SetProcessToDeviceRouting(int pId, Device device);
}