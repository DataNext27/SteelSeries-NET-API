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

This library allows you to take control over the SteelSeries GG app (only Sonar for now).

The library is available via a [nuget package](https://www.nuget.org/packages/Steelseries-NET-API).  
It is also available in the [Releases](https://github.com/mpaperno/SteelSeries-NET-API/releases) tab as a .zip archive for each supported .NET version.

## Features
 - Full Sonar control
   - Mode
   - Volume
   - Mute
   - ChatMix
   - Configs (Can't edit a config)
   - Playback Devices
   - Streamer mode Personal & Stream Mixes
   - Streamer mode Audience Monitoring

## Getting Started
To get started, you only need to create a Sonar Object.
`````csharp
// Create Sonar object
SonarBridge sonarManager = new SonarBridge();

// Wait for GG to start before continuing
sonarManager.WaitUntilSteelSeriesStarted();

// Wait for sonar to start before continuing
sonarManager.WaitUntilSonarStarted();

// Start listening to Sonar Events (optional and require admin rights)
sonarManager.StartListener();
sonarManager.SonarEventManager.OnSonarModeChange += OnModeChangeHandler; // Register event

Mode currentMode = sonarManager.Mode.Get(); // Returns the current mode
sonarManager.VolumeSettings.SetVolume(0.5, Device.Game); // Set the Game Device volume
...
`````
For more example, you can check the [Sample](SteelSeriesAPI.Sample/Program.cs) and the [Tests](SteelSeriesAPI.Tests/Program.cs) folders.  
If you need any sort of Documentation, go check the [Repo's Wiki](https://github.com/DataNext27/SteelSeries-NET-API/wiki) for more information.

## Todo
(Actually not planned as not possible, maybe one day I guess :/ )
- Moments
- Engine
- Settings

If anyone find a way to control these above, feel free to create a pull request or an issue

## Projects Using This API
- [TouchPortal SteelSeries GG Plugin](https://github.com/DataNext27/TouchPortal_SteelSeriesGG) made by DataNext