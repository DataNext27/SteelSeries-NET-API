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
        sonarManager.SonarEventManager.OnSonarModeChange += OnModeChangeHandler; // When the mode is change
        sonarManager.SonarEventManager.OnSonarVolumeChange += OnVolumeChangeHandler; // When the volume of a Sonar Device or Channel is changed
        sonarManager.SonarEventManager.OnSonarMuteChange += OnMuteChangeHandler; // When a Sonar Device or Channel is muted or unmuted
        sonarManager.SonarEventManager.OnSonarConfigChange += OnConfigChangeHandler; // When a new config is set to a Sonar Device
        sonarManager.SonarEventManager.OnSonarChatMixChange += OnChatMixChangeHandler; // When the ChatMix value is changed
        sonarManager.SonarEventManager.OnSonarRedirectionDeviceChange += OnRedirectionDeviceChangeHandler; // When the Redirection Device of a Sonar Device is changed
        sonarManager.SonarEventManager.OnSonarRedirectionStateChange += OnRedirectionStateChangeHandler; // When the Redirection of a Sonar Channel is muted or unmuted
        sonarManager.SonarEventManager.OnSonarAudienceMonitoringChange += OnAudienceMonitoringChangeHandler; // When the Audience Monitoring is muted or unmuted

        // Get current sonar mode
        Mode mode = sonarManager.GetMode();
        // Change sonar mode to Streamer
        sonarManager.SetMode(Mode.Streamer);

        // Get current volume of a Sonar Device
        double vol = sonarManager.GetVolume(Device.Media);
        // Get current volume of a Sonar Channel
        double vol2 = sonarManager.GetVolume(Device.Chat, Channel.Stream);
        // Set the volume of a Sonar Device
        sonarManager.SetVolume(0.75, Device.Game);
        // Set the volume of a Sonar Channel
        sonarManager.SetVolume(0.1, Device.Media, Channel.Monitoring);
        
        // Get the current mute state of a Sonar Device
        bool state = sonarManager.GetMute(Device.Chat);
        bool state2 = sonarManager.GetMute(Device.Master, Channel.Monitoring);
        // Set the current mute state of a Sonar Device
        sonarManager.SetMute(true, Device.Chat); // Mute chat

        // Get audio configs
        List<SonarAudioConfiguration> allConfigs = sonarManager.GetAllAudioConfigurations().ToList(); // Return all configs (A SonarAudioConfiguration contains an Id, a Name and an AssociatedDevice)
        List<SonarAudioConfiguration> mediaConfigs = sonarManager.GetAudioConfigurations(Device.Media).ToList(); // Return all configs of a Sonar Device
        SonarAudioConfiguration currentConfig = sonarManager.GetSelectedAudioConfiguration(Device.Media); // Return the currently used config of a Sonar Device
        // Set the config of a Sonar Device
        sonarManager.SetConfig(Device.Media, "Podcast"); // Using its name
        sonarManager.SetConfig(currentConfig.Id); // Using its id (no need to precise which Sonar Device, one id goes to one Sonar Device)
        
        // Get ChatMix info
        double chatMixBalance = sonarManager.GetChatMixBalance(); // The ChatMix value between -1 and 1
        bool chatMixState = sonarManager.GetChatMixState(); // If ChatMix is usable or not
        // Change ChatMix value
        sonarManager.SetChatMixBalance(0.5); // 0.5 is halfway to Chat
        
        // Get redirection devices (Windows devices)
        List<RedirectionDevice> inputDevices = sonarManager.GetRedirectionDevices(Direction.Input).ToList(); // Input devices (Mics...)
        sonarManager.GetRedirectionDevices(Direction.Output); // Output devices (headset, speakers...)
        sonarManager.GetRedirectionDeviceFromId("{0.0.0.00000000}.{192b4f5b-9cc1-4eb2-b752-c5e15b99d548}"); // Get a redirection device from its id
        RedirectionDevice gameRDevice = sonarManager.GetClassicRedirectionDevice(Device.Game); // Give currently used Redirection Device for classic mode
        sonarManager.GetStreamRedirectionDevice(Channel.Monitoring); // Give currently used Redirection Device for Streamer mode
        sonarManager.GetStreamRedirectionDevice(Device.Mic); // Give currently used Redirection Device for Mic in streamer mode
        // Change redirection devices using their id
        sonarManager.SetClassicRedirectionDevice(gameRDevice.Id, Device.Game);
        sonarManager.SetStreamRedirectionDevice(gameRDevice.Id, Channel.Monitoring);
        sonarManager.SetStreamRedirectionDevice(inputDevices[0].Id, Device.Mic);
        
        // Get the redirections states
        sonarManager.GetRedirectionState(Device.Media, Channel.Monitoring);
        // Change the redirections states
        sonarManager.SetRedirectionState(false, Device.Media, Channel.Monitoring);
        
        // Get Audience Monitoring state
        sonarManager.GetAudienceMonitoringState();
        // Change Audience Monitoring state
        sonarManager.SetAudienceMonitoringState(false);
        
        // Get routed processes of a Sonar Device
        List<RoutedProcess> mediaProcesses = sonarManager.GetRoutedProcess(Device.Media).ToList(); // Will surely return apps like Google Chrome or Spotify
        // Route a process to a Sonar Device using its process ID (pid)
        sonarManager.SetProcessToDeviceRouting(mediaProcesses[0].PId, Device.Media);
    }
    
    static void OnModeChangeHandler(object? sender, SonarModeEvent eventArgs)
    {
        Console.WriteLine("Received Mode Event : " + eventArgs.NewMode);
    }

    static void OnVolumeChangeHandler(object? sender, SonarVolumeEvent eventArgs)
    {
        Console.WriteLine("Received Volume Event : " + eventArgs.Volume + ", " + eventArgs.Mode + ", " + eventArgs.Device + ", " + eventArgs.Channel);
    }

    static void OnMuteChangeHandler(object? sender, SonarMuteEvent eventArgs)
    {
        Console.WriteLine("Received Mute Event : " + eventArgs.Muted + ", " + eventArgs.Mode + ", " + eventArgs.Device + ", " + eventArgs.Channel);
    }

    static void OnConfigChangeHandler(object? sender, SonarConfigEvent eventArgs)
    {
        Console.WriteLine("Received Config Event : " + eventArgs.ConfigId);
    }

    static void OnChatMixChangeHandler(object? sender, SonarChatMixEvent eventArgs)
    {
        Console.WriteLine("Received ChatMix Event : " + eventArgs.Balance);
    }

    static void OnRedirectionDeviceChangeHandler(object? sender, SonarRedirectionDeviceEvent eventArgs)
    {
        Console.WriteLine("Received Redirection Device Event : " + eventArgs.RedirectionDeviceId + ", " + eventArgs.Mode + ", " + eventArgs.Device + ", " + eventArgs.Channel);
    }

    static void OnRedirectionStateChangeHandler(object? sender, SonarRedirectionStateEvent eventArgs)
    {
        Console.WriteLine("Received Redirection State Event : " + eventArgs.State + ", " + eventArgs.Device + ", " + eventArgs.Channel);
    }

    static void OnAudienceMonitoringChangeHandler(object? sender, SonarAudienceMonitoringEvent eventArgs)
    {
        Console.WriteLine("Received Audience Monitoring Event : " + eventArgs.AudienceMonitoringState);
    }
}