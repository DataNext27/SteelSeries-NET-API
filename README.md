# SteelSeries-NET-API

![GitHub Downloads](https://img.shields.io/github/downloads/DataNext27/SteelSeries-NET-API/total?style=for-the-badge&color=6fca00&link=https%3A%2F%2Fgithub.com%2FDataNext27%2FSteelSeries-NET-API%2Freleases)
![GitHub Version](https://img.shields.io/github/v/tag/DataNext27/SteelSeries-NET-API?style=for-the-badge&label=Version&link=https%3A%2F%2Fgithub.com%2FDataNext27%2FSteelSeries-NET-API%2Freleases%2Flatest)
<a href='https://ko-fi.com/M4M2VL6WW' target='_blank'><img height='29' style='border:0px;height:29px;' src='https://storage.ko-fi.com/cdn/brandasset/kofi_button_dark.png' border='0' alt='Buy Me a Coffee at ko-fi.com' /></a> </br>
> This library is **NOT** affiliated in any way with **SteelSeries**  
> I've made it because it was interesting and funny to do, also I wanted to share this project for people to use it for their own projects

This library allows you to take control over the SteelSeries GG app.

The API is available via a [nuget package](https://www.nuget.org/packages/Steelseries-NET-API).  
It is also available in the [Releases](https://github.com/mpaperno/SteelSeries-NET-API/releases) tab as a .zip archive for each supported .NET version.

## Features
 - Full Sonar control

## Getting Started
To get started, you only need to create a Sonar Object.
`````csharp
// Create Sonar object
SonarBridge sonarManager = new SonarBridge();

// Wait for sonar to start before continuing
sonarManager.WaitUntilSonarStarted();

// Start listening to Sonar Events (optional and require admin rights)
sonarManager.StartListener();
sonarManager.SonarEventManager.OnSonarModeChange += OnModeChangeHandler; // Register event

sonarManager.GetMode(); // Returns the current mode
sonarManager.SetVolume(0.5, Device.Game); // Set the Game Device volume
`````
For more example, you can check the [Tests](https://github.com/DataNext27/SteelSeries-NET-API/tree/main/SteelSeriesAPI.Tests) and the [Sample](https://github.com/DataNext27/SteelSeries-NET-API/tree/main/SteelSeriesAPI.Sample) folders.  
If you need any sort of Documentation, go check the [Repo's Wiki](https://github.com/DataNext27/SteelSeries-NET-API/wiki) for more information.

### Some Vocabulary
- Mode : Classic/Stream
- Device : Master/Game/Chat/Media/Aux/Mic
- Channel : *(Streamer mode)* Monitoring/Stream
- Audio Configs : It's in the name
- Redirection States : *(Streamer mode)* Button above sliders to un/mute a channel of a device
- Redirection Device : Device where the sound got by GG is redirected (your headset for example)

## Todo
(Actually not possible, maybe one day i guess :/ )
- Moments
- Engine
- Settings

If anyone find a way to control these above, feel free to create a pull request

## Projects Using This API
- [TouchPortal SteelSeriesGG Plugin](https://github.com/DataNext27/TouchPortal_SteelSeriesGG) made by DataNext