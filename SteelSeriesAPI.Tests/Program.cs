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
        sonarManager.WaitUntilSonarStarted();
        Console.WriteLine(new SonarRetriever().WebServerAddress());

        sonarManager.StartListener();

        // Thread.Sleep(1000);
        // sonarManager.StopListener();

        sonarManager.Event.OnSonarModeChange += OnModeChangeHandler;
        sonarManager.Event.OnSonarVolumeChange += OnVolumeChangeHandler;
        sonarManager.Event.OnSonarMuteChange += OnMuteChangeHandler;
        sonarManager.Event.OnSonarConfigChange += OnConfigChangeHandler;
        sonarManager.Event.OnSonarChatMixChange += OnChatMixChangeHandler;
        sonarManager.Event.OnSonarRedirectionDeviceChange += OnRedirectionDeviceChangeHandler;
        sonarManager.Event.OnSonarRedirectionStateChange += OnRedirectionStateChangeHandler;
        sonarManager.Event.OnSonarAudienceMonitoringChange += OnAudienceMonitoringChangeHandler;

        // new Program().GetTest(sonarManager);
        // new Program().SetTest(sonarManager);
        // new Program().TestMaster(sonarManager);
    }

    void TestMaster(SonarBridge sonarManager)
    {
        
        // Classic
        sonarManager.SetMode(Mode.CLASSIC);
        Console.WriteLine("------ Classic Master Test ------");
        Console.WriteLine(sonarManager.VolumeSettings.GetVolume(Channel.MASTER));
        Console.WriteLine(sonarManager.VolumeSettings.GetMute(Channel.MASTER));
        sonarManager.VolumeSettings.SetVolume(0.25f, Channel.MASTER);
        sonarManager.VolumeSettings.SetVolume(1, Channel.MASTER);
        sonarManager.VolumeSettings.SetMute(true, Channel.MASTER);
        sonarManager.VolumeSettings.SetMute(false, Channel.MASTER);

        // Streamer
        sonarManager.SetMode(Mode.STREAMER);
        Console.WriteLine("------ Streamer Master Test ------");
        Console.WriteLine(sonarManager.VolumeSettings.GetVolume(Channel.MASTER, Mix.MONITORING));
        Console.WriteLine(sonarManager.VolumeSettings.GetVolume(Channel.MASTER, Mix.STREAM));
        Console.WriteLine(sonarManager.VolumeSettings.GetMute(Channel.MASTER, Mix.MONITORING));
        Console.WriteLine(sonarManager.VolumeSettings.GetMute(Channel.MASTER, Mix.STREAM));
        sonarManager.VolumeSettings.SetVolume(0.25f, Channel.MASTER, Mix.MONITORING);
        sonarManager.VolumeSettings.SetVolume(0.25f, Channel.MASTER, Mix.STREAM);
        sonarManager.VolumeSettings.SetVolume(1, Channel.MASTER, Mix.MONITORING);
        sonarManager.VolumeSettings.SetVolume(1, Channel.MASTER, Mix.STREAM);
        sonarManager.VolumeSettings.SetMute(true, Channel.MASTER, Mix.MONITORING);
        sonarManager.VolumeSettings.SetMute(true, Channel.MASTER, Mix.STREAM);
        sonarManager.VolumeSettings.SetMute(false, Channel.MASTER, Mix.MONITORING);
        sonarManager.VolumeSettings.SetMute(false, Channel.MASTER, Mix.STREAM);
        
        sonarManager.SetMode(Mode.CLASSIC);
    }

    void GetTest(SonarBridge sonarManager)
    {

        Console.WriteLine(sonarManager.GetMode());
        Console.WriteLine("" + sonarManager.VolumeSettings.GetVolume(Channel.MASTER) + "  " + sonarManager.VolumeSettings.GetMute(Channel.MASTER));
        Console.WriteLine("" + sonarManager.VolumeSettings.GetVolume(Channel.GAME) + "  " + sonarManager.VolumeSettings.GetMute(Channel.GAME));
        Console.WriteLine("" + sonarManager.VolumeSettings.GetVolume(Channel.CHAT) + "  " + sonarManager.VolumeSettings.GetMute(Channel.CHAT));
        Console.WriteLine("" + sonarManager.VolumeSettings.GetVolume(Channel.MEDIA) + "  " + sonarManager.VolumeSettings.GetMute(Channel.MEDIA));
        Console.WriteLine("" + sonarManager.VolumeSettings.GetVolume(Channel.AUX) + "  " + sonarManager.VolumeSettings.GetMute(Channel.AUX));
        Console.WriteLine("" + sonarManager.VolumeSettings.GetVolume(Channel.MIC) + "  " + sonarManager.VolumeSettings.GetMute(Channel.MIC));
        Console.WriteLine("-----------------Streamer-Monitoring-------------");
        Console.WriteLine("" + sonarManager.VolumeSettings.GetVolume(Channel.MASTER, Mix.MONITORING) + "  " + sonarManager.VolumeSettings.GetMute(Channel.MASTER, Mix.MONITORING));
        Console.WriteLine("" + sonarManager.VolumeSettings.GetVolume(Channel.GAME, Mix.MONITORING) + "  " + sonarManager.VolumeSettings.GetMute(Channel.GAME, Mix.MONITORING));
        Console.WriteLine("" + sonarManager.VolumeSettings.GetVolume(Channel.CHAT, Mix.MONITORING) + "  " + sonarManager.VolumeSettings.GetMute(Channel.CHAT, Mix.MONITORING));
        Console.WriteLine("" + sonarManager.VolumeSettings.GetVolume(Channel.MEDIA, Mix.MONITORING) + "  " + sonarManager.VolumeSettings.GetMute(Channel.MEDIA, Mix.MONITORING));
        Console.WriteLine("" + sonarManager.VolumeSettings.GetVolume(Channel.AUX, Mix.MONITORING) + "  " + sonarManager.VolumeSettings.GetMute(Channel.AUX, Mix.MONITORING));
        Console.WriteLine("" + sonarManager.VolumeSettings.GetVolume(Channel.MIC, Mix.MONITORING) + "  " + sonarManager.VolumeSettings.GetMute(Channel.MIC, Mix.MONITORING));
        Console.WriteLine("-----------------Streamer-Stream-------------");
        Console.WriteLine("" + sonarManager.VolumeSettings.GetVolume(Channel.MASTER, Mix.STREAM) + "  " + sonarManager.VolumeSettings.GetMute(Channel.MASTER, Mix.STREAM));
        Console.WriteLine("" + sonarManager.VolumeSettings.GetVolume(Channel.GAME, Mix.STREAM) + "  " + sonarManager.VolumeSettings.GetMute(Channel.GAME, Mix.STREAM));
        Console.WriteLine("" + sonarManager.VolumeSettings.GetVolume(Channel.CHAT, Mix.STREAM) + "  " + sonarManager.VolumeSettings.GetMute(Channel.CHAT, Mix.STREAM));
        Console.WriteLine("" + sonarManager.VolumeSettings.GetVolume(Channel.MEDIA, Mix.STREAM) + "  " + sonarManager.VolumeSettings.GetMute(Channel.MEDIA, Mix.STREAM));
        Console.WriteLine("" + sonarManager.VolumeSettings.GetVolume(Channel.AUX, Mix.STREAM) + "  " + sonarManager.VolumeSettings.GetMute(Channel.AUX, Mix.STREAM));
        Console.WriteLine("" + sonarManager.VolumeSettings.GetVolume(Channel.MIC, Mix.STREAM) + "  " + sonarManager.VolumeSettings.GetMute(Channel.MIC, Mix.STREAM));
        Console.WriteLine("----AudioConfigs----------");
        foreach (var config in sonarManager.GetAllAudioConfigurations())
        {
            Console.WriteLine(config.Id + ", " + config.Name + ", " + config.AssociatedChannel);
        }
        
        Console.WriteLine("----Media-Configs----------");
        foreach (var config in sonarManager.GetAudioConfigurations(Channel.MEDIA))
        {
            Console.WriteLine(config.Id + ", " + config.Name);
        }

        Console.WriteLine("----Mic-Configs----------");
        foreach (var config in sonarManager.GetAudioConfigurations(Channel.MIC))
        {
            Console.WriteLine(config.Id + ", " + config.Name);
        }

        Console.WriteLine("----Current Media Config----------");
        Console.WriteLine(sonarManager.GetSelectedAudioConfiguration(Channel.MEDIA).Name);
        Console.WriteLine("----Channel from config ID----------");
        Console.WriteLine(sonarManager.GetAudioConfiguration("29ae2c02-792b-4487-863c-dc3e11a7a469").AssociatedChannel);
        Console.WriteLine("--------ChatMix---------");
        Console.WriteLine(sonarManager.GetChatMixBalance());
        Console.WriteLine(sonarManager.GetChatMixState());

        Console.WriteLine("-----Redirection Devices-----------");
        Console.WriteLine("---Output---");
        foreach (var rDevice in sonarManager.GetPlaybackDevices(DataFlow.OUTPUT))
        {
            Console.WriteLine(rDevice.Id + ", " + rDevice.Name);
            foreach (var device in rDevice.AssociatedClassicDevices)
            {
                Console.WriteLine("...." + device);
            }

            foreach (var channel in rDevice.AssociatedStreamDevices)
            {
                Console.WriteLine("...." + channel);
            }
        }

        Console.WriteLine("---Input---");
        foreach (var rDevice in sonarManager.GetPlaybackDevices(DataFlow.INPUT))
        {
            Console.WriteLine(rDevice.Id + ", " + rDevice.Name);
            foreach (var device in rDevice.AssociatedClassicDevices)
            {
                Console.WriteLine("...." + device);
            }

            foreach (var channel in rDevice.AssociatedStreamDevices)
            {
                Console.WriteLine("...." + channel);
            }
        }

        Console.WriteLine("-----Classic Redirection Devices------------");
        PlaybackDevice reDevice = sonarManager.GetClassicPlaybackDevice(Channel.GAME);
        Console.WriteLine(reDevice.Id + ", " + reDevice.Name);
        foreach (var device in reDevice.AssociatedClassicDevices)
        {
            Console.WriteLine("...." + device);
        }

        foreach (var channel in reDevice.AssociatedStreamDevices)
        {
            Console.WriteLine("...." + channel);
        }

        Console.WriteLine("-----Stream Redirection Devices------------");
        PlaybackDevice reDeviceS = sonarManager.GetStreamPlaybackDevice(Channel.MIC); // sonarManager.GetStreamPlaybackDevice(Mix.MONITORING);
        Console.WriteLine(reDeviceS.Id + ", " + reDeviceS.Name);
        foreach (var device in reDeviceS.AssociatedClassicDevices)
        {
            Console.WriteLine("...." + device);
        }

        foreach (var channel in reDeviceS.AssociatedStreamDevices)
        {
            Console.WriteLine("...." + channel);
        }

        Console.WriteLine("-----Redirection Channel From Id");
        // PlaybackDevice someDevice = sonarManager.GetPlaybackDeviceFromId("{0.0.0.00000000}.{453f6e2f-375e-4b36-97b2-2aa55691ab3c}");
        // Console.WriteLine(someDevice.Id + ", " + someDevice.Name + ", " + someDevice.DataFlow);
        // foreach (var associatedClassicDevice in someDevice.AssociatedClassicDevices)
        // {
        //     Console.WriteLine("...." + associatedClassicDevice);
        // }
        //
        // foreach (var associatedStreamDevice in someDevice.AssociatedStreamDevices)
        // {
        //     Console.WriteLine("...." + associatedStreamDevice);
        // }

        Console.WriteLine("-----Redirection States---------");
        Console.WriteLine(sonarManager.GetRedirectionState(Channel.GAME, Mix.MONITORING));
        Console.WriteLine(sonarManager.GetRedirectionState(Channel.CHAT, Mix.MONITORING));
        Console.WriteLine(sonarManager.GetRedirectionState(Channel.MEDIA, Mix.MONITORING));
        Console.WriteLine(sonarManager.GetRedirectionState(Channel.AUX, Mix.MONITORING));
        Console.WriteLine(sonarManager.GetRedirectionState(Channel.MIC, Mix.MONITORING));
        Console.WriteLine(sonarManager.GetRedirectionState(Channel.GAME, Mix.STREAM));
        Console.WriteLine(sonarManager.GetRedirectionState(Channel.CHAT, Mix.STREAM));
        Console.WriteLine(sonarManager.GetRedirectionState(Channel.MEDIA, Mix.STREAM));
        Console.WriteLine(sonarManager.GetRedirectionState(Channel.AUX, Mix.STREAM));
        Console.WriteLine(sonarManager.GetRedirectionState(Channel.MIC, Mix.STREAM));
        Console.WriteLine("-----Audience Monitoring-------");
        Console.WriteLine(sonarManager.GetAudienceMonitoringState());
        Console.WriteLine("-----Routed Processes-----------");
        foreach (Channel device in (Channel[])Enum.GetValues(typeof(Channel)))
        {
            if (device == Channel.MASTER)
            {
                continue;
            }

            Console.WriteLine("-- " + device);
            foreach (var routed in sonarManager.GetRoutedProcess(device))
            {
                Console.WriteLine(routed.Id + ", " + routed.ProcessName + ", " + routed.PId + ", " + routed.State +
                                  ", " + routed.DisplayName);
            }
        }
    }

    void SetTest(SonarBridge sonarManager){

        sonarManager.SetMode(Mode.CLASSIC);
        sonarManager.VolumeSettings.SetVolume(0.4, Channel.MEDIA);
        sonarManager.VolumeSettings.SetMute(false, Channel.MEDIA);
        string configId = sonarManager.GetAudioConfigurations(Channel.MEDIA).FirstOrDefault(config => config.Name == "Default")?.Id;
        sonarManager.SetConfig(configId);
        sonarManager.SetConfig(Channel.MEDIA, "Default");
        sonarManager.SetChatMixBalance(0.5);
        
        var redirectionDevices = sonarManager.GetPlaybackDevices(DataFlow.INPUT);
        redirectionDevices.GetEnumerator().MoveNext();
        sonarManager.SetClassicPlaybackDevice(redirectionDevices.GetEnumerator().Current.Id, Channel.MIC);
        sonarManager.SetStreamPlaybackDevice(redirectionDevices.GetEnumerator().Current.Id, Channel.MIC);
        
        sonarManager.SetRedirectionState(true, Channel.MEDIA, Mix.STREAM);
        sonarManager.SetAudienceMonitoringState(false);
        sonarManager.SetProcessToDeviceRouting(19152, Channel.MIC);
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