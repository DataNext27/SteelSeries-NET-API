using System.Globalization;
using SteelSeriesAPI.Sonar.Events;
using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sonar.Managers;

/// <summary>
/// Manage the different Sonar Events
/// </summary>
public class EventManager
{
    /// <summary>
    /// Notify when the current <see cref="Mode"/> changed
    /// </summary>
    public event EventHandler<SonarModeEvent> OnSonarModeChange = delegate{  };
    
    /// <summary>
    /// Notify when the volume of a <see cref="Channel"/> has changed
    /// </summary>
    public event EventHandler<SonarVolumeEvent> OnSonarVolumeChange = delegate{  };
    
    /// <summary>
    /// Notify when a <see cref="Channel"/> gets un/muted
    /// </summary>
    public event EventHandler<SonarMuteEvent> OnSonarMuteChange = delegate{  };
    
    /// <summary>
    /// Notify when the config of a <see cref="Channel"/> is changed
    /// </summary>
    public event EventHandler<SonarConfigEvent> OnSonarConfigChange = delegate{  };
    
    /// <summary>
    /// Notify when ChatMix value was changed
    /// </summary>
    public event EventHandler<SonarChatMixEvent> OnSonarChatMixChange = delegate{  };
    
    /// <summary>
    /// Notify when the playback device of a <see cref="Channel"/> is changed
    /// </summary>
    public event EventHandler<SonarPlaybackDeviceEvent> OnSonarPlaybackDeviceChange = delegate{  };
    
    /// <summary>
    /// Notify when an audio process is routed to a new <see cref="Channel"/>
    /// </summary>
    public event EventHandler<SonarRoutedProcessEvent> OnSonarRoutedProcessChange = delegate{  }; 
    
    /// <summary>
    /// Notify when a <see cref="Mix"/> gets de/activated
    /// </summary>
    public event EventHandler<SonarMixEvent> OnSonarMixChange = delegate{  };
    
    /// <summary>
    /// Notify when the audience monitoring state is changed
    /// </summary>
    public event EventHandler<SonarAudienceMonitoringEvent> OnSonarAudienceMonitoringChange = delegate{  };

    internal void HandleEvent(string path)
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
            case SonarPlaybackDeviceEvent sonarPlaybackDeviceEvent:
                OnSonarPlaybackDeviceChange(this, sonarPlaybackDeviceEvent);
                break;
            case SonarRoutedProcessEvent sonarRoutedProcessEvent:
                OnSonarRoutedProcessChange(this, sonarRoutedProcessEvent);
                break;
            case SonarMixEvent sonarMixEvent:
                OnSonarMixChange(this, sonarMixEvent);
                break;
            case SonarAudienceMonitoringEvent sonarAudienceMonitoringEvent:
                OnSonarAudienceMonitoringChange(this, sonarAudienceMonitoringEvent);
                break;
        }
    }

    private EventArgs PathResolver(string path)
    {
        string[] subs = path.Split("/");
        EventArgs eventArgs = null!;
        
        switch (subs[1])
        {
            case "mode":
                eventArgs = new SonarModeEvent
                    { NewMode = (Mode)ModeExtensions.FromDictKey(subs[2], ModeMapChoice.StreamDict)! };
                break;
            case "volumeSettings":
                switch (subs[2])
                {
                    case "classic":
                        switch (subs[4])
                        {
                            case "Volume":
                                eventArgs = new SonarVolumeEvent
                                {
                                    Volume = double.Parse(subs[5], CultureInfo.InvariantCulture.NumberFormat), 
                                    Mode = Mode.CLASSIC,
                                    Channel = (Channel)ChannelExtensions.FromDictKey(subs[3], ChannelMapChoice.HttpDict)!
                                };
                                break;
                            case"Mute":
                                eventArgs = new SonarMuteEvent
                                {
                                    Muted = Convert.ToBoolean(subs[5]),
                                    Mode = Mode.CLASSIC,
                                    Channel = (Channel)ChannelExtensions.FromDictKey(subs[3], ChannelMapChoice.HttpDict)!
                                };
                                break;
                        }
                        break;
                    case "streamer":
                        switch (subs[5])
                        {
                            case "volume":
                                eventArgs = new SonarVolumeEvent
                                {
                                    Volume = double.Parse(subs[6], CultureInfo.InvariantCulture.NumberFormat),
                                    Mode = Mode.STREAMER,
                                    Channel = (Channel)ChannelExtensions.FromDictKey(subs[4], ChannelMapChoice.HttpDict)!,
                                    Mix = (Mix)MixExtensions.FromDictKey(subs[3])!
                                };
                                break;
                            case "isMuted":
                                eventArgs = new SonarMuteEvent
                                {
                                    Muted = Convert.ToBoolean(subs[6]),
                                    Mode = Mode.STREAMER,
                                    Channel = (Channel)ChannelExtensions.FromDictKey(subs[4], ChannelMapChoice.HttpDict)!,
                                    Mix = (Mix)MixExtensions.FromDictKey(subs[3])!
                                };
                                break;
                        }
                        break;
                }
                break;
            case "configs":
                if (!(subs.Length < 3))
                {
                    eventArgs = new SonarConfigEvent { ConfigId = subs[2] };
                }
                break;
            case "classicRedirections":
                eventArgs = new SonarPlaybackDeviceEvent
                {
                    PlaybackDeviceId = subs[4].Replace("%7B", "{").Replace("%7D", "}"),
                    Mode = Mode.CLASSIC,
                    Device = (Channel)ChannelExtensions.FromDictKey(subs[2], ChannelMapChoice.ChannelDict)!
                };
                break;
            case "streamRedirections":
                if (subs[2] == "isStreamMonitoringEnabled")
                {
                    eventArgs = new SonarAudienceMonitoringEvent { NewState = Convert.ToBoolean(subs[3]) };
                    break;
                }

                switch (subs[3])
                {
                    case "deviceId":
                        if (subs[2] == "mic")
                        {
                            eventArgs = new SonarPlaybackDeviceEvent
                            {
                                PlaybackDeviceId = subs[4].Replace("%7B", "{").Replace("%7D", "}"),
                                Mode = Mode.STREAMER,
                                Device = Channel.MIC
                            };
                            break;
                        }
                        
                        eventArgs = new SonarPlaybackDeviceEvent
                        {
                            PlaybackDeviceId = subs[4].Replace("%7B", "{").Replace("%7D", "}"),
                            Mode = Mode.STREAMER,
                            Channel = (Mix)MixExtensions.FromDictKey(subs[2])!
                        };
                        break;
                    case "redirections":
                        eventArgs = new SonarMixEvent
                        {
                            NewState = Convert.ToBoolean(subs[6]),
                            Channel = (Channel)ChannelExtensions.FromDictKey(subs[4])!,
                            Mix = (Mix)MixExtensions.FromDictKey(subs[2])!
                        };
                        break;
                }
                break;
            case "AudioDeviceRouting":
                eventArgs = new SonarRoutedProcessEvent(subs[3].Replace("%7B", "{").Replace("%7D", "}"))
                {
                    ProcessId = Convert.ToInt32(subs[4])
                };
                break;
            default:
                if (subs[1].StartsWith("chatMix"))
                {
                    eventArgs = new SonarChatMixEvent { Balance = Convert.ToDouble(subs[1].Split("=")[1], CultureInfo.InvariantCulture) };
                }
                break;
        }
        
        return eventArgs;
    }
}