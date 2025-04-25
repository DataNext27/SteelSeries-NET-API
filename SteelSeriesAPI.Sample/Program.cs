using SteelSeriesAPI.Sonar;
using SteelSeriesAPI.Sonar.Events;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Sample;

class Program
{
    static void Main(string[] args)
    {
        // Create a Sonar Object to control Sonar
        SonarBridge sonarManager = new SonarBridge();
        
        // Wait until GG and Sonar are both started before continuing
        sonarManager.WaitUntilSonarStarted();
        
        // If I want to detect changes made on GG, I can use the listener (require admin rights)
        sonarManager.StartListener();
        // Then I register the events I want (I've put them all to demonstrate)
        sonarManager.Event.OnSonarModeChange += OnModeChangeHandler; // When the mode is change
        sonarManager.Event.OnSonarVolumeChange += OnVolumeChangeHandler; // When the volume of a Sonar Channel or Mix is changed
        sonarManager.Event.OnSonarMuteChange += OnMuteChangeHandler; // When a Sonar Channel or Mix is muted or unmuted
        sonarManager.Event.OnSonarConfigChange += OnConfigChangeHandler; // When a new config is set to a Sonar Channel
        sonarManager.Event.OnSonarChatMixChange += OnChatMixChangeHandler; // When the ChatMix value is changed
        sonarManager.Event.OnSonarRedirectionDeviceChange += OnRedirectionDeviceChangeHandler; // When the Redirection Channel of a Sonar Channel is changed
        sonarManager.Event.OnSonarRedirectionStateChange += OnRedirectionStateChangeHandler; // When the Redirection of a Sonar Mix is muted or unmuted
        sonarManager.Event.OnSonarAudienceMonitoringChange += OnAudienceMonitoringChangeHandler; // When the Audience Monitoring is muted or unmuted

        // Get current sonar mode
        Mode mode = sonarManager.GetMode();
        // Change sonar mode to Streamer
        sonarManager.SetMode(Mode.STREAMER);

        // Get current volume of a Sonar Channel
        double vol = sonarManager.VolumeSettings.GetVolume(Channel.MEDIA);
        // Get current volume of a Sonar Mix
        double vol2 = sonarManager.VolumeSettings.GetVolume(Channel.CHAT, Mix.STREAM);
        // Set the volume of a Sonar Channel
        sonarManager.VolumeSettings.SetVolume(0.75, Channel.GAME);
        // Set the volume of a Sonar Mix
        sonarManager.VolumeSettings.SetVolume(0.1, Channel.MEDIA, Mix.MONITORING);
        
        // Get the current mute state of a Sonar Channel
        bool state = sonarManager.VolumeSettings.GetMute(Channel.CHAT);
        bool state2 = sonarManager.VolumeSettings.GetMute(Channel.MASTER, Mix.MONITORING);
        // Set the current mute state of a Sonar Channel
        sonarManager.VolumeSettings.SetMute(true, Channel.CHAT); // Mute chat

        // Get audio configs
        List<SonarAudioConfiguration> allConfigs = sonarManager.Configurations.GetAllAudioConfigurations().ToList(); // Return all configs (A SonarAudioConfiguration contains an Id, a Name and an AssociatedChannel)
        List<SonarAudioConfiguration> mediaConfigs = sonarManager.Configurations.GetAudioConfigurations(Channel.MEDIA).ToList(); // Return all configs of a Sonar Channel
        SonarAudioConfiguration currentConfig = sonarManager.Configurations.GetSelectedAudioConfiguration(Channel.MEDIA); // Return the currently used config of a Sonar Channel
        // Set the config of a Sonar Channel
        sonarManager.Configurations.SetConfigByName(Channel.MEDIA, "Podcast"); // Using its name
        sonarManager.Configurations.SetConfig(currentConfig.Id); // Using its id (no need to precise which Sonar Channel, one id goes to one Sonar Channel)
        sonarManager.Configurations.SetConfig(currentConfig); // Or you can just directly give the config
        
        // Get ChatMix info
        double chatMixBalance = sonarManager.GetChatMixBalance(); // The ChatMix value between -1 and 1
        bool chatMixState = sonarManager.GetChatMixState(); // If ChatMix is usable or not
        // Change ChatMix value
        sonarManager.SetChatMixBalance(0.5); // 0.5 is halfway to Chat
        
        // Get playback devices (Windows devices)
        List<PlaybackDevice> inputDevices = sonarManager.GetPlaybackDevices(DataFlow.INPUT).ToList(); // Input devices (Mics...)
        sonarManager.GetPlaybackDevices(DataFlow.OUTPUT); // Output devices (headset, speakers...)
        sonarManager.GetPlaybackDeviceFromId("{0.0.0.00000000}.{192b4f5b-9cc1-4eb2-b752-c5e15b99d548}"); // Get a redirection channel from its id
        PlaybackDevice gameRDevice = sonarManager.GetClassicPlaybackDevice(Channel.GAME); // Give currently used Redirection Channel for classic mode
        sonarManager.GetStreamPlaybackDevice(Mix.MONITORING); // Give currently used Redirection Channel for Streamer mode
        sonarManager.GetStreamPlaybackDevice(Channel.MIC); // Give currently used Redirection Channel for Mic in streamer mode
        // Change playback devices using their id
        sonarManager.SetClassicPlaybackDevice(gameRDevice.Id, Channel.GAME);
        sonarManager.SetStreamPlaybackDevice(gameRDevice.Id, Mix.MONITORING);
        sonarManager.SetStreamPlaybackDevice(inputDevices[0].Id, Channel.MIC);
        
        // Get the redirections states
        sonarManager.GetRedirectionState(Channel.MEDIA, Mix.MONITORING);
        // Change the redirections states
        sonarManager.SetRedirectionState(false, Channel.MEDIA, Mix.MONITORING);
        
        // Get Audience Monitoring state
        sonarManager.GetAudienceMonitoringState();
        // Change Audience Monitoring state
        sonarManager.SetAudienceMonitoringState(false);
        
        // Get routed processes of a Sonar Channel
        List<RoutedProcess> mediaProcesses = sonarManager.GetRoutedProcess(Channel.MEDIA).ToList(); // Will surely return apps like Google Chrome or Spotify
        // Route a process to a Sonar Channel using its process ID (pid)
        sonarManager.SetProcessToDeviceRouting(mediaProcesses[0].PId, Channel.MEDIA);
    }
    
    static void OnModeChangeHandler(object? sender, SonarModeEvent eventArgs)
    {
        Console.WriteLine("Received Mode Event : " + eventArgs.NewMode);
    }

    static void OnVolumeChangeHandler(object? sender, SonarVolumeEvent eventArgs)
    {
        Console.WriteLine("Received Volume Event : " + eventArgs.Volume + ", " + eventArgs.Mode + ", " + eventArgs.Channel + ", " + eventArgs.Mix);
    }

    static void OnMuteChangeHandler(object? sender, SonarMuteEvent eventArgs)
    {
        Console.WriteLine("Received Mute Event : " + eventArgs.Muted + ", " + eventArgs.Mode + ", " + eventArgs.Channel + ", " + eventArgs.Mix);
    }

    static void OnConfigChangeHandler(object? sender, SonarConfigEvent eventArgs)
    {
        Console.WriteLine("Received Config Event : " + eventArgs.ConfigId);
    }

    static void OnChatMixChangeHandler(object? sender, SonarChatMixEvent eventArgs)
    {
        Console.WriteLine("Received ChatMix Event : " + eventArgs.Balance);
    }

    static void OnRedirectionDeviceChangeHandler(object? sender, SonarPlaybackDeviceEvent eventArgs)
    {
        Console.WriteLine("Received Redirection Channel Event : " + eventArgs.RedirectionDeviceId + ", " + eventArgs.Mode + ", " + eventArgs.Device + ", " + eventArgs.Channel);
    }

    static void OnRedirectionStateChangeHandler(object? sender, SonarRedirectionStateEvent eventArgs)
    {
        Console.WriteLine("Received Redirection State Event : " + eventArgs.State + ", " + eventArgs.Channel + ", " + eventArgs.Mix);
    }

    static void OnAudienceMonitoringChangeHandler(object? sender, SonarAudienceMonitoringEvent eventArgs)
    {
        Console.WriteLine("Received Audience Monitoring Event : " + eventArgs.AudienceMonitoringState);
    }
}