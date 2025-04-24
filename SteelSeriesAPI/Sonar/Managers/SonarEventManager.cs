using System.Globalization;
using SteelSeriesAPI.Sonar.Events;
using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sonar.Managers;

public class SonarEventManager
{
    /// <summary>
    /// Notify when mode changed
    /// </summary>
    public event EventHandler<SonarModeEvent> OnSonarModeChange = delegate{  };
    
    /// <summary>
    /// Notify when a volume of a device changed
    /// </summary>
    public event EventHandler<SonarVolumeEvent> OnSonarVolumeChange = delegate{  };
    
    /// <summary>
    /// Notify when a device is un/muted
    /// </summary>
    public event EventHandler<SonarMuteEvent> OnSonarMuteChange = delegate{  };
    
    /// <summary>
    /// Notify when the config of a device is changed
    /// </summary>
    public event EventHandler<SonarConfigEvent> OnSonarConfigChange = delegate{  };
    
    /// <summary>
    /// Notify when ChatMix value was changed
    /// </summary>
    public event EventHandler<SonarChatMixEvent> OnSonarChatMixChange = delegate{  };
    
    /// <summary>
    /// Notify when a redirection device of a device is changed
    /// </summary>
    public event EventHandler<SonarRedirectionDeviceEvent> OnSonarRedirectionDeviceChange = delegate{  };
    
    /// <summary>
    /// Notify when a redirection state is changed
    /// </summary>
    public event EventHandler<SonarRedirectionStateEvent> OnSonarRedirectionStateChange = delegate{  };
    
    /// <summary>
    /// Notify when the audience monitoring state is changed
    /// </summary>
    public event EventHandler<SonarAudienceMonitoringEvent> OnSonarAudienceMonitoringChange = delegate{  };

    public void HandleEvent(string path)
    {
        var eventMessage = PathResolver(path);
        switch (eventMessage)
        {
            case SonarModeEvent sonarModeEvent:
                OnSonarModeChange(this, sonarModeEvent);
                break;
            case SonarVolumeEvent sonarVolumeEvent:
                OnSonarVolumeChange(this, sonarVolumeEvent);
                break;
            case SonarMuteEvent sonarMuteEvent:
                OnSonarMuteChange(this, sonarMuteEvent);
                break;
            case SonarConfigEvent sonarConfigEvent:
                OnSonarConfigChange(this, sonarConfigEvent);
                break;
            case SonarChatMixEvent sonarChatMixEvent:
                OnSonarChatMixChange(this, sonarChatMixEvent);
                break;
            case SonarRedirectionDeviceEvent sonarRedirectionDeviceEvent:
                OnSonarRedirectionDeviceChange(this, sonarRedirectionDeviceEvent);
                break;
            case SonarRedirectionStateEvent sonarRedirectionStateEvent:
                OnSonarRedirectionStateChange(this, sonarRedirectionStateEvent);
                break;
            case SonarAudienceMonitoringEvent sonarAudienceMonitoringEvent:
                OnSonarAudienceMonitoringChange(this, sonarAudienceMonitoringEvent);
                break;
        }
    }

    private EventArgs PathResolver(string path)
    {
        string[] subs = path.Split("/");
        EventArgs eventArgs = null;
        
        switch (subs[1])
        {
            case "mode":
                eventArgs = new SonarModeEvent()
                    { NewMode = (Mode)ModeExtensions.FromDictKey(subs[2], ModeMapChoice.StreamDict) };
                break;
            case "volumeSettings":
                switch (subs[2])
                {
                    case "classic":
                        switch (subs[4])
                        {
                            case "Volume":
                                eventArgs = new SonarVolumeEvent()
                                {
                                    Volume = double.Parse(subs[5], CultureInfo.InvariantCulture.NumberFormat), 
                                    Mode = Mode.Classic,
                                    Device = (Device)DeviceExtensions.FromDictKey(subs[3], DeviceMapChoice.HttpDict)
                                };
                                break;
                            case"Mute":
                                eventArgs = new SonarMuteEvent()
                                {
                                    Muted = Convert.ToBoolean(subs[5]),
                                    Mode = Mode.Classic,
                                    Device = (Device)DeviceExtensions.FromDictKey(subs[3], DeviceMapChoice.HttpDict)
                                };
                                break;
                        }
                        break;
                    case "streamer":
                        switch (subs[5])
                        {
                            case "volume":
                                eventArgs = new SonarVolumeEvent()
                                {
                                    Volume = double.Parse(subs[6], CultureInfo.InvariantCulture.NumberFormat),
                                    Mode = Mode.Streamer,
                                    Device = (Device)DeviceExtensions.FromDictKey(subs[4], DeviceMapChoice.HttpDict),
                                    Channel = (Channel)ChannelExtensions.FromDictKey(subs[3])
                                };
                                break;
                            case "isMuted":
                                eventArgs = new SonarMuteEvent()
                                {
                                    Muted = Convert.ToBoolean(subs[6]),
                                    Mode = Mode.Streamer,
                                    Device = (Device)DeviceExtensions.FromDictKey(subs[4], DeviceMapChoice.HttpDict),
                                    Channel = (Channel)ChannelExtensions.FromDictKey(subs[3])
                                };
                                break;
                        }
                        break;
                }
                break;
            case "configs":
                if (!(subs.Length < 3))
                {
                    eventArgs = new SonarConfigEvent() { ConfigId = subs[2] };
                }
                break;
            case "classicRedirections":
                eventArgs = new SonarRedirectionDeviceEvent()
                {
                    RedirectionDeviceId = subs[4].Replace("%7B", "{").Replace("%7D", "}"),
                    Mode = Mode.Classic,
                    Device = (Device)DeviceExtensions.FromDictKey(subs[2], DeviceMapChoice.DeviceDict)
                };
                break;
            case "streamRedirections":
                if (subs[2] == "isStreamMonitoringEnabled")
                {
                    eventArgs = new SonarAudienceMonitoringEvent() { AudienceMonitoringState = Convert.ToBoolean(subs[3]) };
                    break;
                }

                switch (subs[3])
                {
                    case "deviceId":
                        if (subs[2] == "mic")
                        {
                            eventArgs = new SonarRedirectionDeviceEvent()
                            {
                                RedirectionDeviceId = subs[4].Replace("%7B", "{").Replace("%7D", "}"),
                                Mode = Mode.Streamer,
                                Device = Device.Mic
                            };
                            break;
                        }
                        
                        eventArgs = new SonarRedirectionDeviceEvent()
                        {
                            RedirectionDeviceId = subs[4].Replace("%7B", "{").Replace("%7D", "}"),
                            Mode = Mode.Streamer,
                            Channel = (Channel)ChannelExtensions.FromDictKey(subs[2])
                        };
                        break;
                    case "redirections":
                        eventArgs = new SonarRedirectionStateEvent()
                        {
                            State = Convert.ToBoolean(subs[6]),
                            Device = (Device)DeviceExtensions.FromDictKey(subs[4]),
                            Channel = (Channel)ChannelExtensions.FromDictKey(subs[2])
                        };
                        break;
                }
                break;
            default:
                if (subs[1].StartsWith("chatMix"))
                {
                    eventArgs = new SonarChatMixEvent() { Balance = Convert.ToDouble(subs[1].Split("=")[1], CultureInfo.InvariantCulture) };
                }
                break;
        }
        
        return eventArgs;
    }
}