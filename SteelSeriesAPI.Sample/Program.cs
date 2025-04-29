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
        
        // Wait until GG is started before continuing
        sonarManager.WaitUntilSteelSeriesStarted();
        // Wait until GG and Sonar are both started before continuing
        sonarManager.WaitUntilSonarStarted();
        
        // If you want to detect changes made on GG, you can use the listener (require admin rights)
        sonarManager.StartListener();
        // Then you register the events you need (I've put them all to demonstrate)
        sonarManager.Events.OnSonarModeChange += OnModeChangeHandler; // When the mode gets changed
        sonarManager.Events.OnSonarVolumeChange += OnVolumeChangeHandler; // When the volume of a Sonar Channel or Mix gets changed
        sonarManager.Events.OnSonarMuteChange += OnMuteChangeHandler; // When a Sonar Channel or Mix gets muted or unmuted
        sonarManager.Events.OnSonarConfigChange += OnConfigChangeHandler; // When a new config is set to a Sonar Channel
        sonarManager.Events.OnSonarChatMixChange += OnChatMixChangeHandler; // When the ChatMix value gets changed
        sonarManager.Events.OnSonarPlaybackDeviceChange += OnPlaybackDeviceChangeHandler; // When the Redirection Channel of a Sonar Channel is changed
        sonarManager.Events.OnSonarRoutedProcessChange += OnRoutedProcessChangeHandler; // When a routed process gets routed to a new Sonar Channel
        sonarManager.Events.OnSonarMixChange += OnMixChangeHandler; // When the Mix of a Sonar Channem gets activated or deactivated
        sonarManager.Events.OnSonarAudienceMonitoringChange += OnAudienceMonitoringChangeHandler; // When the Audience Monitoring gets muted or unmuted

        // Get current sonar mode
        Mode mode = sonarManager.Mode.Get();
        // Change sonar mode to Streamer
        sonarManager.Mode.Set(Mode.STREAMER);

        // Get current volume of a Sonar Channel
        double vol = sonarManager.VolumeSettings.GetVolume(Channel.MEDIA);
        // Get current volume of a Sonar Mix
        double vol2 = sonarManager.VolumeSettings.GetVolume(Channel.CHAT, Mix.STREAM);
        // Set the volume of a Sonar Channel
        sonarManager.VolumeSettings.SetVolume(0.75, Channel.GAME);
        // Set the volume of a Sonar Mix
        sonarManager.VolumeSettings.SetVolume(0.1, Channel.MEDIA, Mix.PERSONAL);
        
        // Get the current mute state of a Sonar Channel
        bool state = sonarManager.VolumeSettings.GetMute(Channel.CHAT);
        bool state2 = sonarManager.VolumeSettings.GetMute(Channel.MASTER, Mix.PERSONAL);
        // Set the current mute state of a Sonar Channel
        sonarManager.VolumeSettings.SetMute(true, Channel.CHAT); // Mute chat

        // Get audio configs
        List<SonarAudioConfiguration> allConfigs = sonarManager.Configurations.GetAllAudioConfigurations().ToList(); // Return all configs (A SonarAudioConfiguration contains an Id, a Name and an AssociatedChannel)
        List<SonarAudioConfiguration> mediaConfigs = sonarManager.Configurations.GetAudioConfigurations(Channel.MEDIA).ToList(); // Return all configs of a Sonar Channel
        SonarAudioConfiguration currentConfig = sonarManager.Configurations.GetSelectedAudioConfiguration(Channel.MEDIA); // Return the currently used config of a Sonar Channel
        // Set the config of a Sonar Channel
        sonarManager.Configurations.SetConfigByName(Channel.MEDIA, "Podcast"); // Using its name
        sonarManager.Configurations.SetConfig(currentConfig); // Using directly the config object
        sonarManager.Configurations.SetConfig(currentConfig.Id); // Or Using its id (no need to precise which Sonar Channel, one id = one config = one Sonar Channel)
        
        // Get ChatMix info
        double chatMixBalance = sonarManager.ChatMix.GetBalance(); // The ChatMix value between -1 and 1
        bool chatMixState = sonarManager.ChatMix.GetState(); // If ChatMix is usable or not
        // Change ChatMix value
        sonarManager.ChatMix.SetBalance(0.5); // 0.5 is halfway to Chat
        
        // Get playback devices (Windows devices)
        List<PlaybackDevice> playbackDevices = sonarManager.PlaybackDevices.GetAllPlaybackDevices().ToList(); // All playback devices
        List<PlaybackDevice> inputDevices = sonarManager.PlaybackDevices.GetInputPlaybackDevices().ToList(); // Input devices (Mics...)
        List<PlaybackDevice> outputDevices = sonarManager.PlaybackDevices.GetOutputPlaybackDevices().ToList(); // Output devices (headset, speakers...)
        PlaybackDevice gameDevice = sonarManager.PlaybackDevices.GetPlaybackDevice(Channel.GAME); // Get the currently used Playback device of a Channel
        sonarManager.PlaybackDevices.GetPlaybackDevice(Mix.STREAM); // Get the currently used Playback device of a Mix
        sonarManager.PlaybackDevices.GetPlaybackDevice(Channel.MIC, Mode.STREAMER); // Get the currently used Playback device of the streamer mode Mic
        sonarManager.PlaybackDevices.GetPlaybackDevice("{0.0.0.00000000}.{192b4f5b-9cc1-4eb2-b752-c5e15b99d548}"); // Get a playback device from its id
        // Change playback devices
        sonarManager.PlaybackDevices.SetPlaybackDevice(gameDevice, Channel.GAME); // Using the playback device object
        sonarManager.PlaybackDevices.SetPlaybackDevice("{0.0.0.00000000}.{192b4f5b-9cc1-4eb2-b752-c5e15b99d548}", Channel.AUX); // Using the playback device ID
        
        // Get the mixes states
        sonarManager.Mix.GetState(Channel.MEDIA, Mix.PERSONAL);
        // Change the mixes states
        sonarManager.Mix.Activate(Channel.MEDIA, Mix.PERSONAL);
        sonarManager.Mix.Deactivate(Channel.CHAT, Mix.STREAM);
        sonarManager.Mix.SetState(false, Channel.MEDIA, Mix.PERSONAL); // Same as deactivating here
        
        // Get Audience Monitoring state
        sonarManager.AudienceMonitoring.GetState();
        // Change Audience Monitoring state
        sonarManager.AudienceMonitoring.SetState(false);

        // Get all routed processes whether they are active, inactive or expired
        List<RoutedProcess> allProcesses = sonarManager.RoutedProcesses.GetAllRoutedProcesses().ToList();
        // Get all active routed processes (currently in use)
        List<RoutedProcess> allActiveProcesses = sonarManager.RoutedProcesses.GetAllActiveRoutedProcesses().ToList();
        // Same but for a specific channel
        List<RoutedProcess> gameProcesses = sonarManager.RoutedProcesses.GetRoutedProcesses(Channel.GAME).ToList(); // Will surely return apps like Minecraft...
        List<RoutedProcess> mediaActiveProcesses = sonarManager.RoutedProcesses.GetActiveRoutedProcesses(Channel.MEDIA).ToList(); // Will surely return apps like Google Chrome or Spotify
        // Same idea but by giving the ID of an audio process
        sonarManager.RoutedProcesses.GetRoutedProcessesById(2063);
        sonarManager.RoutedProcesses.GetActiveRoutedProcessesById(10548);
        // Route a process to a Sonar Channel using the RoutedProcess object
        sonarManager.RoutedProcesses.RouteProcessToChannel(mediaActiveProcesses[0], Channel.AUX);
        // Route a process to a Sonar Channel using its process ID (pid)
        sonarManager.RoutedProcesses.RouteProcessToChannel(15482, Channel.MEDIA);
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

    static void OnPlaybackDeviceChangeHandler(object? sender, SonarPlaybackDeviceEvent eventArgs)
    {
        Console.WriteLine("Received Redirection Channel Event : " + eventArgs.PlaybackDeviceId + ", " + eventArgs.Mode + ", " + eventArgs.Channel + ", " + eventArgs.Mix);
    }
    
    static void OnRoutedProcessChangeHandler(object? sender, SonarRoutedProcessEvent eventArgs)
    {
        Console.WriteLine("Received Routed Process Event : " + eventArgs.ProcessId + ", " + eventArgs.NewChannel);
    }

    static void OnMixChangeHandler(object? sender, SonarMixEvent eventArgs)
    {
        Console.WriteLine("Received Redirection State Event : " + eventArgs.NewState + ", " + eventArgs.Channel + ", " + eventArgs.Mix);
    }

    static void OnAudienceMonitoringChangeHandler(object? sender, SonarAudienceMonitoringEvent eventArgs)
    {
        Console.WriteLine("Received Audience Monitoring Event : " + eventArgs.NewState);
    }
}