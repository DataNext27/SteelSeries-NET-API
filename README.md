# SteelSeries-NET-API

[![GitHub Downloads](https://img.shields.io/github/downloads/DataNext27/SteelSeries-NET-API/total?style=for-the-badge&color=6fca00)](https://github.com/DataNext27/SteelSeries-NET-API/releases/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Steelseries-NET-API?style=for-the-badge&label=Nuget%20Downloads&color=%23004880)](https://www.nuget.org/packages/Steelseries-NET-API)
[![GitHub Version](https://img.shields.io/github/v/tag/DataNext27/SteelSeries-NET-API?style=for-the-badge&label=Version)](https://github.com/DataNext27/SteelSeries-NET-API/releases/latest/)
[![GitHub License](https://img.shields.io/github/license/DataNext27/SteelSeries-NET-API?style=for-the-badge&color=red)](https://github.com/DataNext27/SteelSeries-NET-API/blob/main/LICENSE)
[![.NET Version](https://img.shields.io/badge/.NET-9.0-512cd4?style=for-the-badge)](https://dotnet.microsoft.com/fr-fr/download/dotnet/9.0)
[![.NET Version](https://img.shields.io/badge/.NET-8.0-512cd4?style=for-the-badge)](https://dotnet.microsoft.com/fr-fr/download/dotnet/8.0)
[![.NET Version](https://img.shields.io/badge/.NET-7.0-512cd4?style=for-the-badge)](https://dotnet.microsoft.com/fr-fr/download/dotnet/7.0)
[![Ko-fi](https://img.shields.io/badge/Support_me_on-Ko--fi-FF6433?style=for-the-badge&logo=ko-fi)](https://ko-fi.com/M4M2VL6WW)
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

Mode currentMode = sonarManager.GetMode(); // Returns the current mode
sonarManager.SetVolume(0.5, Device.Game); // Set the Game Device volume
...
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

If anyone find a way to control these above, feel free to create a pull request or an issue

## Projects Using This API
- [TouchPortal SteelSeries GG Plugin](https://github.com/DataNext27/TouchPortal_SteelSeriesGG) made by DataNext