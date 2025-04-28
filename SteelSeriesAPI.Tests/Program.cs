using SteelSeriesAPI.Sonar;
using SteelSeriesAPI.Sonar.Events;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Models;

namespace SteelSeriesAPI.Tests;

class Program
{
    static void Main(string[] args)
    {
        SonarBridge sonarManager = new SonarBridge();
        sonarManager.WaitUntilSteelSeriesStarted();
        sonarManager.WaitUntilSonarStarted();
        Console.WriteLine(SonarRetriever.Instance.WebServerAddress());
        
        // sonarManager.StartListener();

        // Thread.Sleep(1000);
        // sonarManager.StopListener();

        // sonarManager.Events.OnSonarModeChange += OnModeChangeHandler;
        // sonarManager.Events.OnSonarVolumeChange += OnVolumeChangeHandler;
        // sonarManager.Events.OnSonarMuteChange += OnMuteChangeHandler;
        // sonarManager.Events.OnSonarConfigChange += OnConfigChangeHandler;
        // sonarManager.Events.OnSonarChatMixChange += OnChatMixChangeHandler;
        // sonarManager.Events.OnSonarPlaybackDeviceChange += OnPlaybackDeviceChangeHandler;
        // sonarManager.Events.OnSonarRoutedProcessChange += OnRoutedProcessChangeHandler;
        // sonarManager.Events.OnSonarMixChange += OnMixChangeHandler;
        // sonarManager.Events.OnSonarAudienceMonitoringChange += OnAudienceMonitoringChangeHandler;

        // Save current settings
        var _mode = sonarManager.Mode.Get();
        var _chatmix = sonarManager.ChatMix.GetBalance();
        
        sonarManager.Mode.Set(Mode.CLASSIC);
        
        foreach (Channel channel in (Channel[])Enum.GetValues(typeof(Channel)))
        {
            Console.WriteLine("------ " + channel + " ------");
            if (channel == Channel.MASTER)
            {
                var _volume = sonarManager.VolumeSettings.GetVolume(channel);
                var _mute = sonarManager.VolumeSettings.GetMute(channel);
                
                Console.WriteLine("Volume test...");
                sonarManager.VolumeSettings.SetVolume(_volume + 0.2, channel);
                Thread.Sleep(1000);
                sonarManager.VolumeSettings.SetVolume(_volume, channel);
            
                Thread.Sleep(500);
        
                Console.WriteLine("Mute test...");
                sonarManager.VolumeSettings.SetMute(!_mute, channel);
                Thread.Sleep(1000);
                sonarManager.VolumeSettings.SetMute(_mute, channel);
            }
            else
            {
                var _volume = sonarManager.VolumeSettings.GetVolume(channel);
                var _mute = sonarManager.VolumeSettings.GetMute(channel);
                var _config = sonarManager.Configurations.GetSelectedAudioConfiguration(channel);
                var _playbackDevice = sonarManager.PlaybackDevices.GetPlaybackDevice(channel);
                var _routedProcesses = new List<RoutedProcess>(sonarManager.RoutedProcesses.GetActiveRoutedProcesses(channel));
                
                Console.WriteLine("Volume test...");
                sonarManager.VolumeSettings.SetVolume(_volume + 0.2, channel);
                Thread.Sleep(1000);
                sonarManager.VolumeSettings.SetVolume(_volume, channel);
                
                Thread.Sleep(500);
            
                Console.WriteLine("Mute test...");
                sonarManager.VolumeSettings.SetMute(!_mute, channel);
                Thread.Sleep(1000);
                sonarManager.VolumeSettings.SetMute(_mute, channel);
                
                Thread.Sleep(500);
            
                Console.WriteLine("Config test...");
                sonarManager.Configurations.SetConfig(sonarManager.Configurations.GetAudioConfigurations(channel).ToList()[4]);
                Thread.Sleep(1000);
                sonarManager.Configurations.SetConfig(_config);
                
                Thread.Sleep(500);
            
                Console.WriteLine("Playback Device test...");
                if (channel == Channel.MIC)
                {
                    sonarManager.PlaybackDevices.SetPlaybackDevice(sonarManager.PlaybackDevices.GetInputPlaybackDevices().First(), channel);
                }
                else
                {
                    sonarManager.PlaybackDevices.SetPlaybackDevice(sonarManager.PlaybackDevices.GetOutputPlaybackDevices().First(), channel);
                }
                Thread.Sleep(1000);
                sonarManager.PlaybackDevices.SetPlaybackDevice(_playbackDevice, channel);

                Console.WriteLine("Routed Processes test...");
                foreach (var r in _routedProcesses)
                {
                    if (channel == Channel.GAME)
                    {
                        sonarManager.RoutedProcesses.RouteProcessToChannel(r, Channel.AUX);
                    }
                    else
                    {
                        sonarManager.RoutedProcesses.RouteProcessToChannel(r, Channel.GAME);
                    }
                }
                Thread.Sleep(1000);
                foreach (var r in _routedProcesses)
                {
                    sonarManager.RoutedProcesses.RouteProcessToChannel(r, channel);
                }
            }
        }

        if (sonarManager.ChatMix.GetState())
        {
            Console.WriteLine("Chat Mix Test...");
            sonarManager.ChatMix.SetBalance(_chatmix + 0.2);
            Thread.Sleep(1000);
            sonarManager.ChatMix.SetBalance(_chatmix);
        }
        
        sonarManager.Mode.Set(Mode.STREAMER);

        foreach (Channel channel in (Channel[])Enum.GetValues(typeof(Channel)))
        {
            foreach (Mix mix in (Mix[])Enum.GetValues(typeof(Mix)))
            {
                if (channel == Channel.MASTER)
                {
                    
                }
                else
                {
                    
                }
            }
        }
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
        Console.WriteLine("Received Redirection Channel Event : " + eventArgs.PlaybackDeviceId + ", " + eventArgs.Mode + ", " + eventArgs.Device + ", " + eventArgs.Channel);
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