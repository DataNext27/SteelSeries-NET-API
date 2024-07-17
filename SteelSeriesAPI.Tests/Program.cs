using SteelSeriesAPI.EventArgs;
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

        // sonarManager.StartListener();
        
        // do
        // {
        //     continue;
        // } while (Console.ReadKey(true).Key != ConsoleKey.Enter);
        // sonarManager.StopListener();
        // Console.WriteLine("End");
        
        Console.WriteLine(sonarManager.GetMode());
        Console.WriteLine("" + sonarManager.GetVolume(Device.Master) + "  " + sonarManager.GetMute(Device.Master));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Game) + "  " + sonarManager.GetMute(Device.Game));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Chat) + "  " + sonarManager.GetMute(Device.Chat));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Media) + "  " + sonarManager.GetMute(Device.Media));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Aux) + "  " + sonarManager.GetMute(Device.Aux));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Mic) + "  " + sonarManager.GetMute(Device.Mic));
        Console.WriteLine("-----------------Streamer-Monitoring-------------");
        Console.WriteLine("" + sonarManager.GetVolume(Device.Master, Mode.Streamer) + "  " + sonarManager.GetMute(Device.Master, Mode.Streamer));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Game, Mode.Streamer) + "  " + sonarManager.GetMute(Device.Game, Mode.Streamer));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Chat, Mode.Streamer) + "  " + sonarManager.GetMute(Device.Chat, Mode.Streamer));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Media, Mode.Streamer) + "  " + sonarManager.GetMute(Device.Media, Mode.Streamer));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Aux, Mode.Streamer) + "  " + sonarManager.GetMute(Device.Aux, Mode.Streamer));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Mic, Mode.Streamer) + "  " + sonarManager.GetMute(Device.Mic, Mode.Streamer));
        Console.WriteLine("-----------------Streamer-Stream-------------");
        Console.WriteLine("" + sonarManager.GetVolume(Device.Master, Mode.Streamer, Channel.Stream) + "  " + sonarManager.GetMute(Device.Master, Mode.Streamer, Channel.Stream));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Game, Mode.Streamer, Channel.Stream) + "  " + sonarManager.GetMute(Device.Game, Mode.Streamer, Channel.Stream));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Chat, Mode.Streamer, Channel.Stream) + "  " + sonarManager.GetMute(Device.Chat, Mode.Streamer, Channel.Stream));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Media, Mode.Streamer, Channel.Stream) + "  " + sonarManager.GetMute(Device.Media, Mode.Streamer, Channel.Stream));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Aux, Mode.Streamer, Channel.Stream) + "  " + sonarManager.GetMute(Device.Aux, Mode.Streamer, Channel.Stream));
        Console.WriteLine("" + sonarManager.GetVolume(Device.Mic, Mode.Streamer, Channel.Stream) + "  " + sonarManager.GetMute(Device.Mic, Mode.Streamer, Channel.Stream));
        Console.WriteLine("----AudioConfigs----------");
        foreach (var config in sonarManager.GetAllAudioConfigurations())
        {
            Console.WriteLine(config.Id + ", " + config.Name + ", " + config.AssociatedDevice);
        }
        Console.WriteLine("----Mic-Configs----------");
        foreach (var config in sonarManager.GetAudioConfigurations(Device.Mic))
        {
            Console.WriteLine(config.Id + ", " + config.Name);
        }
        Console.WriteLine("----Current Media Config----------");
        Console.WriteLine(sonarManager.GetSelectedAudioConfiguration(Device.Media).Name);
        Console.WriteLine("----Device from config ID----------");
        Console.WriteLine(sonarManager.GetDeviceFromAudioConfigurationId("29ae2c02-792b-4487-863c-dc3e11a7a469"));
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
        RedirectionDevice reDeviceS = sonarManager.GetStreamRedirectionDevice(Channel.Monitoring);
        Console.WriteLine(reDeviceS.Id + ", " + reDeviceS.Name);
        foreach (var device in reDeviceS.AssociatedClassicDevices)
        {
            Console.WriteLine("...." + device);
        }
        
        foreach (var channel in reDeviceS.AssociatedStreamDevices)
        {
            Console.WriteLine("...." + channel);
        }
        
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



        sonarManager.SetMode(Mode.Classic);
        sonarManager.SetVolume(0.2, Device.Media);
        sonarManager.SetMute(false, Device.Media);
        var configs = sonarManager.GetAudioConfigurations(Device.Game);
        configs.GetEnumerator().MoveNext();
        sonarManager.SetConfig(configs.GetEnumerator().Current.Id);
        sonarManager.SetConfig(Device.Media, "Default");
        sonarManager.SetChatMixBalance(0.5);
        var redirectionDevices = sonarManager.GetRedirectionDevices(Direction.Input);
        redirectionDevices.GetEnumerator().MoveNext();
        sonarManager.SetClassicRedirectionDevice(redirectionDevices.GetEnumerator().Current.Id, Device.Mic);
        sonarManager.SetStreamRedirectionDevice(redirectionDevices.GetEnumerator().Current.Id, Device.Mic);
        
        sonarManager.SetRedirectionState(true, Device.Media, Channel.Stream);
        sonarManager.SetAudienceMonitoringState(false);


        /* TODO -
        sonarManager.OnSteelSeriesSonarEvent += OnSteelSeriesEventHandler;

        GetRedircetionDeviceFromId(); // deviceId   return RedirectionDevice
        GetRoutedProcesses(); // Device     return List<RoutedProcess>


        SetProcessToDeviceRouting(); // PID, Device
        */
    }

    public static void OnSteelSeriesEventHandler(object sender, SonarEventArgs eventArgs)
    {
        
    }
}