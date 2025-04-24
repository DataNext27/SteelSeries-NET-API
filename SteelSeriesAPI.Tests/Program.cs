using SteelSeriesAPI.Events;
using SteelSeriesAPI.Sonar;
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

        sonarManager.SonarEventManager.OnSonarModeChange += OnModeChangeHandler;
        sonarManager.SonarEventManager.OnSonarVolumeChange += OnVolumeChangeHandler;
        sonarManager.SonarEventManager.OnSonarMuteChange += OnMuteChangeHandler;
        sonarManager.SonarEventManager.OnSonarConfigChange += OnConfigChangeHandler;
        sonarManager.SonarEventManager.OnSonarChatMixChange += OnChatMixChangeHandler;
        sonarManager.SonarEventManager.OnSonarRedirectionDeviceChange += OnRedirectionDeviceChangeHandler;
        sonarManager.SonarEventManager.OnSonarRedirectionStateChange += OnRedirectionStateChangeHandler;
        sonarManager.SonarEventManager.OnSonarAudienceMonitoringChange += OnAudienceMonitoringChangeHandler;

        // new Program().GetTest(sonarManager);
        // new Program().SetTest(sonarManager);
        // new Program().TestMaster(sonarManager);
    }

    void TestMaster(SonarBridge sonarManager)
    {
        
        // Classic
        sonarManager.SetMode(Mode.Classic);
        Console.WriteLine("------ Classic Master Test ------");
        Console.WriteLine(sonarManager.GetVolume(Device.Master));
        Console.WriteLine(sonarManager.GetMute(Device.Master));
        sonarManager.SetVolume(0.25f, Device.Master);
        sonarManager.SetVolume(1, Device.Master);
        sonarManager.SetMute(true, Device.Master);
        sonarManager.SetMute(false, Device.Master);

        // Streamer
        sonarManager.SetMode(Mode.Streamer);
        Console.WriteLine("------ Streamer Master Test ------");
        Console.WriteLine(sonarManager.GetVolume(Device.Master, Channel.Monitoring));
        Console.WriteLine(sonarManager.GetVolume(Device.Master, Channel.Stream));
        Console.WriteLine(sonarManager.GetMute(Device.Master, Channel.Monitoring));
        Console.WriteLine(sonarManager.GetMute(Device.Master, Channel.Stream));
        sonarManager.SetVolume(0.25f, Device.Master, Channel.Monitoring);
        sonarManager.SetVolume(0.25f, Device.Master, Channel.Stream);
        sonarManager.SetVolume(1, Device.Master, Channel.Monitoring);
        sonarManager.SetVolume(1, Device.Master, Channel.Stream);
        sonarManager.SetMute(true, Device.Master, Channel.Monitoring);
        sonarManager.SetMute(true, Device.Master, Channel.Stream);
        sonarManager.SetMute(false, Device.Master, Channel.Monitoring);
        sonarManager.SetMute(false, Device.Master, Channel.Stream);
        
        sonarManager.SetMode(Mode.Classic);
    }

    void GetTest(SonarBridge sonarManager)
    {

        Console.WriteLine(sonarManager.GetMode());
        Console.WriteLine("" + sonarManager.GetVolume(Device.Master) + "  " + sonarManager.GetMute(Device.Master));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Game) + "  " + sonarManager.GetMute(Device.Game));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Chat) + "  " + sonarManager.GetMute(Device.Chat));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Media) + "  " + sonarManager.GetMute(Device.Media));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Aux) + "  " + sonarManager.GetMute(Device.Aux));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Mic) + "  " + sonarManager.GetMute(Device.Mic));
        Console.WriteLine("-----------------Streamer-Monitoring-------------");
        Console.WriteLine("" + sonarManager.GetVolume(Device.Master, Channel.Monitoring) + "  " + sonarManager.GetMute(Device.Master, Channel.Monitoring));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Game, Channel.Monitoring) + "  " + sonarManager.GetMute(Device.Game, Channel.Monitoring));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Chat, Channel.Monitoring) + "  " + sonarManager.GetMute(Device.Chat, Channel.Monitoring));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Media, Channel.Monitoring) + "  " + sonarManager.GetMute(Device.Media, Channel.Monitoring));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Aux, Channel.Monitoring) + "  " + sonarManager.GetMute(Device.Aux, Channel.Monitoring));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Mic, Channel.Monitoring) + "  " + sonarManager.GetMute(Device.Mic, Channel.Monitoring));
        Console.WriteLine("-----------------Streamer-Stream-------------");
        Console.WriteLine("" + sonarManager.GetVolume(Device.Master, Channel.Stream) + "  " + sonarManager.GetMute(Device.Master, Channel.Stream));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Game, Channel.Stream) + "  " + sonarManager.GetMute(Device.Game, Channel.Stream));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Chat, Channel.Stream) + "  " + sonarManager.GetMute(Device.Chat, Channel.Stream));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Media, Channel.Stream) + "  " + sonarManager.GetMute(Device.Media, Channel.Stream));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Aux, Channel.Stream) + "  " + sonarManager.GetMute(Device.Aux, Channel.Stream));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Mic, Channel.Stream) + "  " + sonarManager.GetMute(Device.Mic, Channel.Stream));
        Console.WriteLine("----AudioConfigs----------");
        foreach (var config in sonarManager.GetAllAudioConfigurations())
        {
            Console.WriteLine(config.Id + ", " + config.Name + ", " + config.AssociatedDevice);
        }
        
        Console.WriteLine("----Media-Configs----------");
        foreach (var config in sonarManager.GetAudioConfigurations(Device.Media))
        {
            Console.WriteLine(config.Id + ", " + config.Name);
        }

        Console.WriteLine("----Mic-Configs----------");
        foreach (var config in sonarManager.GetAudioConfigurations(Device.Mic))
        {
            Console.WriteLine(config.Id + ", " + config.Name);
        }

        Console.WriteLine("----Current Media Config----------");
        Console.WriteLine(sonarManager.GetSelectedAudioConfiguration(Device.Media).Name);
        Console.WriteLine("----Device from config ID----------");
        Console.WriteLine(sonarManager.GetAudioConfiguration("29ae2c02-792b-4487-863c-dc3e11a7a469").AssociatedDevice);
        Console.WriteLine("--------ChatMix---------");
        Console.WriteLine(sonarManager.GetChatMixBalance());
        Console.WriteLine(sonarManager.GetChatMixState());

        Console.WriteLine("-----Redirection Devices-----------");
        Console.WriteLine("---Output---");
        foreach (var rDevice in sonarManager.GetRedirectionDevices(Direction.Output))
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
        foreach (var rDevice in sonarManager.GetRedirectionDevices(Direction.Input))
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
        RedirectionDevice reDevice = sonarManager.GetClassicRedirectionDevice(Device.Game);
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
        RedirectionDevice reDeviceS = sonarManager.GetStreamRedirectionDevice(Device.Mic); // sonarManager.GetStreamRedirectionDevice(Channel.Monitoring);
        Console.WriteLine(reDeviceS.Id + ", " + reDeviceS.Name);
        foreach (var device in reDeviceS.AssociatedClassicDevices)
        {
            Console.WriteLine("...." + device);
        }

        foreach (var channel in reDeviceS.AssociatedStreamDevices)
        {
            Console.WriteLine("...." + channel);
        }

        Console.WriteLine("-----Redirection Device From Id");
        // RedirectionDevice someDevice = sonarManager.GetRedirectionDeviceFromId("{0.0.0.00000000}.{453f6e2f-375e-4b36-97b2-2aa55691ab3c}");
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
        Console.WriteLine(sonarManager.GetRedirectionState(Device.Game, Channel.Monitoring));
        Console.WriteLine(sonarManager.GetRedirectionState(Device.Chat, Channel.Monitoring));
        Console.WriteLine(sonarManager.GetRedirectionState(Device.Media, Channel.Monitoring));
        Console.WriteLine(sonarManager.GetRedirectionState(Device.Aux, Channel.Monitoring));
        Console.WriteLine(sonarManager.GetRedirectionState(Device.Mic, Channel.Monitoring));
        Console.WriteLine(sonarManager.GetRedirectionState(Device.Game, Channel.Stream));
        Console.WriteLine(sonarManager.GetRedirectionState(Device.Chat, Channel.Stream));
        Console.WriteLine(sonarManager.GetRedirectionState(Device.Media, Channel.Stream));
        Console.WriteLine(sonarManager.GetRedirectionState(Device.Aux, Channel.Stream));
        Console.WriteLine(sonarManager.GetRedirectionState(Device.Mic, Channel.Stream));
        Console.WriteLine("-----Audience Monitoring-------");
        Console.WriteLine(sonarManager.GetAudienceMonitoringState());
        Console.WriteLine("-----Routed Processes-----------");
        foreach (Device device in (Device[])Enum.GetValues(typeof(Device)))
        {
            if (device == Device.Master)
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

        sonarManager.SetMode(Mode.Classic);
        sonarManager.SetVolume(0.4, Device.Media);
        sonarManager.SetMute(false, Device.Media);
        string configId = sonarManager.GetAudioConfigurations(Device.Media).FirstOrDefault(config => config.Name == "Default")?.Id;
        sonarManager.SetConfig(configId);
        sonarManager.SetConfig(Device.Media, "Default");
        sonarManager.SetChatMixBalance(0.5);
        
        var redirectionDevices = sonarManager.GetRedirectionDevices(Direction.Input);
        redirectionDevices.GetEnumerator().MoveNext();
        sonarManager.SetClassicRedirectionDevice(redirectionDevices.GetEnumerator().Current.Id, Device.Mic);
        sonarManager.SetStreamRedirectionDevice(redirectionDevices.GetEnumerator().Current.Id, Device.Mic);
        
        sonarManager.SetRedirectionState(true, Device.Media, Channel.Stream);
        sonarManager.SetAudienceMonitoringState(false);
        sonarManager.SetProcessToDeviceRouting(19152, Device.Mic);
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