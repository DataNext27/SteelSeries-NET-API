using SteelSeriesAPI.Sonar;
using SteelSeriesAPI.Sonar.Events;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Exceptions;
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
        
        sonarManager.StartListener();

        // Thread.Sleep(1000);
        // sonarManager.StopListener();

        sonarManager.Events.OnSonarModeChange += OnModeChangeHandler;
        sonarManager.Events.OnSonarVolumeChange += OnVolumeChangeHandler;
        sonarManager.Events.OnSonarMuteChange += OnMuteChangeHandler;
        sonarManager.Events.OnSonarConfigChange += OnConfigChangeHandler;
        sonarManager.Events.OnSonarChatMixChange += OnChatMixChangeHandler;
        sonarManager.Events.OnSonarPlaybackDeviceChange += OnPlaybackDeviceChangeHandler;
        sonarManager.Events.OnSonarRoutedProcessChange += OnRoutedProcessChangeHandler;
        sonarManager.Events.OnSonarMixChange += OnMixChangeHandler;
        sonarManager.Events.OnSonarAudienceMonitoringChange += OnAudienceMonitoringChangeHandler;

        // Save current settings
        var mode = sonarManager.Mode.Get();
        var chatmix = sonarManager.ChatMix.GetBalance();
        var audienceMonitoring = sonarManager.AudienceMonitoring.GetState();
        
        sonarManager.Mode.Set(Mode.CLASSIC);
        
        foreach (Channel channel in (Channel[])Enum.GetValues(typeof(Channel)))
        {
            Console.WriteLine("------ " + channel + " ------");
            if (channel == Channel.MASTER)
            {
                var volume = sonarManager.VolumeSettings.GetVolume(channel);
                var mute = sonarManager.VolumeSettings.GetMute(channel);
                
                Console.WriteLine("Volume test...");
                sonarManager.VolumeSettings.SetVolume(volume + 0.2, channel);
                Thread.Sleep(1000);
                sonarManager.VolumeSettings.SetVolume(volume, channel);
            
                Thread.Sleep(500);
        
                Console.WriteLine("Mute test...");
                sonarManager.VolumeSettings.SetMute(!mute, channel);
                Thread.Sleep(1000);
                sonarManager.VolumeSettings.SetMute(mute, channel);
            }
            else
            {
                var volume = sonarManager.VolumeSettings.GetVolume(channel);
                var mute = sonarManager.VolumeSettings.GetMute(channel);
                var config = sonarManager.Configurations.GetSelectedAudioConfiguration(channel);
                var playbackDevice = sonarManager.PlaybackDevices.GetPlaybackDevice(channel);
                var routedProcesses = new List<RoutedProcess>(sonarManager.RoutedProcesses.GetActiveRoutedProcesses(channel));
                
                Console.WriteLine("Volume test...");
                sonarManager.VolumeSettings.SetVolume(volume + 0.2, channel);
                Thread.Sleep(1000);
                sonarManager.VolumeSettings.SetVolume(volume, channel);
                
                Thread.Sleep(500);
            
                Console.WriteLine("Mute test...");
                sonarManager.VolumeSettings.SetMute(!mute, channel);
                Thread.Sleep(1000);
                sonarManager.VolumeSettings.SetMute(mute, channel);
                
                Thread.Sleep(500);
            
                Console.WriteLine("Config test...");
                sonarManager.Configurations.SetConfig(sonarManager.Configurations.GetAudioConfigurations(channel).ToList()[4]);
                Thread.Sleep(1000);
                sonarManager.Configurations.SetConfig(config);
                
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
                sonarManager.PlaybackDevices.SetPlaybackDevice(playbackDevice, channel);

                Console.WriteLine("Routed Processes test...");
                foreach (var r in routedProcesses)
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
                foreach (var r in routedProcesses)
                {
                    sonarManager.RoutedProcesses.RouteProcessToChannel(r, channel);
                }
            }
        }

        if (sonarManager.ChatMix.GetState())
        {
            Console.WriteLine("Chat Mix Test...");
            sonarManager.ChatMix.SetBalance(chatmix + 0.2);
            Thread.Sleep(1000);
            sonarManager.ChatMix.SetBalance(chatmix);
        }
        else
        {
            Console.WriteLine("Chat Mix disabled. Can't do test");
        }
        
        sonarManager.Mode.Set(Mode.STREAMER);

        foreach (Channel channel in (Channel[])Enum.GetValues(typeof(Channel)))
        {
            foreach (Mix mix in (Mix[])Enum.GetValues(typeof(Mix)))
            {
                Console.WriteLine("------ " + channel + " - " + mix + " ------");
                if (channel == Channel.MASTER)
                {
                    var volume = sonarManager.VolumeSettings.GetVolume(channel, mix);
                    var mute = sonarManager.VolumeSettings.GetMute(channel, mix);
                
                    Console.WriteLine("Volume test...");
                    sonarManager.VolumeSettings.SetVolume(volume - 0.2, channel, mix);
                    Thread.Sleep(1000);
                    sonarManager.VolumeSettings.SetVolume(volume, channel, mix);
            
                    Thread.Sleep(500);
        
                    Console.WriteLine("Mute test...");
                    sonarManager.VolumeSettings.SetMute(!mute, channel, mix);
                    Thread.Sleep(1000);
                    sonarManager.VolumeSettings.SetMute(mute, channel, mix);
                }
                else
                {
                    var volume = sonarManager.VolumeSettings.GetVolume(channel, mix);
                    var mute = sonarManager.VolumeSettings.GetMute(channel, mix);
                    var redirection = sonarManager.Mix.GetState(channel, mix);
                    
                    Console.WriteLine("Volume test...");
                    sonarManager.VolumeSettings.SetVolume(volume - 0.2, channel, mix);
                    Thread.Sleep(1000);
                    sonarManager.VolumeSettings.SetVolume(volume, channel, mix);
            
                    Thread.Sleep(500);
        
                    Console.WriteLine("Mute test...");
                    sonarManager.VolumeSettings.SetMute(!mute, channel, mix);
                    Thread.Sleep(1000);
                    sonarManager.VolumeSettings.SetMute(mute, channel, mix);
                    
                    Console.WriteLine("Mix test...");
                    sonarManager.Mix.SetState(!redirection, channel, mix);
                    Thread.Sleep(1000);
                    sonarManager.Mix.SetState(redirection, channel, mix);
                }
            }
        }
        
        Console.WriteLine("Playback Device test...");
        foreach (Mix mix in (Mix[])Enum.GetValues(typeof(Mix)))
        {
            try
            {
                Console.WriteLine(mix);
                var playbackDevice = sonarManager.PlaybackDevices.GetPlaybackDevice(mix);
                sonarManager.PlaybackDevices.SetPlaybackDevice(sonarManager.PlaybackDevices.GetOutputPlaybackDevices().First(), mix);
                Thread.Sleep(1000);
                sonarManager.PlaybackDevices.SetPlaybackDevice(playbackDevice, mix);
            }
            catch (PlaybackDeviceNotFoundException e)
            {
                Console.WriteLine(e);
                Console.WriteLine("No playback device set for : " + mix);
            }
        }

        try
        {
            Console.WriteLine(Channel.MIC + " " + Mode.STREAMER);
            var micPlaybackDevice = sonarManager.PlaybackDevices.GetPlaybackDevice(Channel.MIC, Mode.STREAMER);
            sonarManager.PlaybackDevices.SetPlaybackDevice(sonarManager.PlaybackDevices.GetOutputPlaybackDevices().First(), Channel.MIC, Mode.STREAMER);
            Thread.Sleep(1000);
            sonarManager.PlaybackDevices.SetPlaybackDevice(micPlaybackDevice, Channel.MIC, Mode.STREAMER);
        }
        catch (PlaybackDeviceNotFoundException e)
        {
            Console.WriteLine(e);
            Console.WriteLine("No playback device set for : " + Channel.MIC + Mode.STREAMER);
        }
        
        Console.WriteLine("Audience Monitoring Test...");
        sonarManager.AudienceMonitoring.SetState(!audienceMonitoring);
        Thread.Sleep(1000);
        sonarManager.AudienceMonitoring.SetState(audienceMonitoring);
        
        sonarManager.Mode.Set(mode);
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