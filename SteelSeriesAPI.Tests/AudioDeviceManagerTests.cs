using System.Text.Json;
using SteelSeriesAPI.Core;
using SteelSeriesAPI.Sonar;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Managers;
using Xunit;

namespace SteelSeriesAPI.Tests;

public class AudioDeviceManagerTests
{
    // Entries taken from the real audioDevices dump (2026-08-25, GG 118)
    private const string DevicesDump =
        """
        [
          {
            "friendlyName": "SteelSeries Sonar - Chat (SteelSeries Sonar Virtual Audio Device)",
            "id": "{0.0.0.00000000}.{06e6c961-3d64-40f2-a617-72bdcbac9265}",
            "dataFlow": "render",
            "role": "chatRender",
            "channels": 2,
            "defaultRole": "communications",
            "fwUpdateRequired": false,
            "state": "active",
            "isVad": true
          },
          {
            "friendlyName": "Haut-parleurs (High Definition Audio Device)",
            "id": "{0.0.0.00000000}.{584e246f-4ae6-417c-acbb-c9f90fee29f1}",
            "dataFlow": "render",
            "role": "none",
            "channels": 2,
            "defaultRole": "all",
            "fwUpdateRequired": false,
            "state": "active",
            "isVad": false
          },
          {
            "friendlyName": "Line input (High Definition Audio Device)",
            "id": "{0.0.1.00000000}.{28da6e04-ea4b-4391-b05c-e505e5c60d50}",
            "dataFlow": "capture",
            "role": "none",
            "channels": 2,
            "defaultRole": "console",
            "fwUpdateRequired": false,
            "state": "active",
            "isVad": false
          },
          {
            "friendlyName": "SteelSeries Sonar - Microphone (SteelSeries Sonar Virtual Audio Device)",
            "id": "{0.0.1.00000000}.{dd3c92e1-f4e3-4171-a5b6-6995e5de70b3}",
            "dataFlow": "capture",
            "role": "chatCapture",
            "channels": 2,
            "defaultRole": "all",
            "fwUpdateRequired": false,
            "state": "active",
            "isVad": true
          }
        ]
        """;

    [Fact]
    public async Task GetAllAsync_RealDump_ParsesAllDevices()
    {
        var transport = new FakeTransport().With(SonarRoutes.AudioDevices, DevicesDump);
        var manager = new AudioDeviceManager(transport);

        var devices = await manager.GetAllAsync();

        Assert.Equal(4, devices.Count);

        var sonarChat = Assert.Single(devices, d => d.Name.Contains("Sonar - Chat"));
        Assert.True(sonarChat.IsSonarVirtual);
        Assert.Equal(Channel.Chat, sonarChat.SonarChannel);       // role "chatRender" -> Chat
        Assert.Equal(AudioDataFlow.Render, sonarChat.DataFlow);

        var speakers = Assert.Single(devices, d => d.Name.StartsWith("Haut-parleurs"));
        Assert.False(speakers.IsSonarVirtual);
        Assert.Null(speakers.SonarChannel);                        // physical device: no Sonar channel

        var sonarMic = Assert.Single(devices, d => d.SonarChannel == Channel.Mic);
        Assert.Equal(AudioDataFlow.Capture, sonarMic.DataFlow);    // role "chatCapture" -> Mic
    }

    [Fact]
    public async Task GetAllAsync_RenderWithoutVirtual_ReturnsRedirectionCandidates()
    {
        // The exact list to offer when picking a classic redirection target
        var transport = new FakeTransport().With(SonarRoutes.AudioDevices, DevicesDump);
        var manager = new AudioDeviceManager(transport);

        var candidates = await manager.GetAllAsync(AudioDataFlow.Render);

        var speakers = Assert.Single(candidates);
        Assert.Equal("Haut-parleurs (High Definition Audio Device)", speakers.Name);
    }

    [Fact]
    public async Task GetAllAsync_IncludeSonarVirtual_KeepsVirtualDevices()
    {
        var transport = new FakeTransport().With(SonarRoutes.AudioDevices, DevicesDump);
        var manager = new AudioDeviceManager(transport);

        var all = await manager.GetAllAsync(AudioDataFlow.Capture, includeSonarVirtual: true);

        Assert.Equal(2, all.Count); // Entrée de ligne + Sonar Microphone
        Assert.Contains(all, d => d.IsSonarVirtual);
    }

    [Fact]
    public void ParseDevices_UnknownDataFlowOrMissingId_IsSkipped()
    {
        // A future GG update adding a new flow, or a malformed entry, must never break the listing
        const string withOddities =
            """
            [
              { "friendlyName": "Weird future device", "id": "x", "dataFlow": "loopback", "isVad": false },
              { "friendlyName": "No id at all", "dataFlow": "render", "isVad": false },
              { "friendlyName": "Valid one", "id": "y", "dataFlow": "render", "isVad": false }
            ]
            """;

        var devices = AudioDeviceManager.ParseDevices(Json(withOddities));

        var valid = Assert.Single(devices);
        Assert.Equal("Valid one", valid.Name);
    }

    [Fact]
    public void ParseDevices_MissingFriendlyName_FallsBackToId()
    {
        const string nameless = """[{ "id": "some-id", "dataFlow": "render", "isVad": false }]""";

        var devices = AudioDeviceManager.ParseDevices(Json(nameless));

        Assert.Equal("some-id", Assert.Single(devices).Name);
    }

    private static JsonElement Json(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }
}