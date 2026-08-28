using System.Text.Json;
using SteelSeriesAPI.Core;
using SteelSeriesAPI.Sonar;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Managers;
using Xunit;

namespace SteelSeriesAPI.Tests;

public class AppRoutingManagerTests
{
    // Trimmed from the real GET AudioDeviceRouting dump (2026-08-25):
    // the Game VAD with an active Brave session, the Media VAD with its ghost, a physical device.
    private const string RoutingDump =
        """
        [
          {
            "deviceId": "{0.0.0.00000000}.{5baa93cb-fefe-420e-b470-17d24a6f0719}",
            "role": "game",
            "dataFlow": "render",
            "audioSessions": [
              { "id": "x|#%b{A9EF3FD9}|1%b#", "processName": "Idle", "processId": 0,
                "isSystemSound": true, "state": "inactive", "displayName": "Idle",
                "isRoutingErrorProne": false, "routingErrorDetected": false },
              { "id": "x|brave.exe%b|1%b7244", "processName": "brave", "processId": 7244,
                "isSystemSound": false, "state": "active", "displayName": "Brave",
                "isRoutingErrorProne": false, "routingErrorDetected": false }
            ]
          },
          {
            "deviceId": "{0.0.0.00000000}.{6587c8e4-f610-4fb7-8075-667cefcb5c05}",
            "role": "media",
            "dataFlow": "render",
            "audioSessions": [
              { "id": "y|brave.exe%b|1%b7244", "processName": "brave", "processId": 7244,
                "isSystemSound": false, "state": "inactive", "displayName": "Brave",
                "isRoutingErrorProne": false, "routingErrorDetected": false }
            ]
          },
          {
            "deviceId": "{0.0.1.00000000}.{28da6e04-ea4b-4391-b05c-e505e5c60d50}",
            "role": "none",
            "dataFlow": "capture",
            "audioSessions": []
          }
        ]
        """;

    // Real WebSocket payload captured on 2026-08-26 (the "data" part of AUDIO_SESSION_OPENED_DATA)
    private const string SessionOpenedData =
        """
        {"deviceId":"{0.0.0.00000000}.{6587c8e4-f610-4fb7-8075-667cefcb5c05}","role":"media","dataFlow":"render",
         "audioSessions":[{"id":"z","processName":"brave","processId":7244,"isSystemSound":false,
         "state":"active","displayName":"Brave","isRoutingErrorProne":false,"routingErrorDetected":true}]}
        """;

    // ---------------- Parsing ----------------

    [Fact]
    public async Task GetRoutingsAsync_RealDump_ParsesDevicesAndSessions()
    {
        var transport = new FakeTransport().With(SonarRoutes.AudioDeviceRouting, RoutingDump);
        var manager = new AppRoutingManager(transport);

        var routings = await manager.GetRoutingsAsync();

        Assert.Equal(3, routings.Count);

        var game = Assert.Single(routings, r => r.Channel == Channel.Game);
        Assert.Equal(2, game.Sessions.Count);
        var brave = Assert.Single(game.Sessions, s => !s.IsSystemSound);
        Assert.Equal("Brave", brave.DisplayName);
        Assert.Equal(7244, brave.ProcessId);
        Assert.True(brave.IsActive);

        // Ghost sessions from past routings must be parsed too, as inactive
        var media = Assert.Single(routings, r => r.Channel == Channel.Media);
        Assert.False(Assert.Single(media.Sessions).IsActive);

        // "none" role resolves to a null channel, not a crash
        Assert.Contains(routings, r => r.Channel is null && r.DataFlow == AudioDataFlow.Capture);
    }

    [Fact]
    public void ParseRouting_WebSocketSessionPayload_SharesTheShape()
    {
        // The AUDIO_SESSION_OPENED/CLOSED event payload is one AudioDeviceRouting entry
        var routing = AppRoutingManager.ParseRouting(Json(SessionOpenedData));

        Assert.NotNull(routing);
        Assert.Equal(Channel.Media, routing!.Channel);
        Assert.Equal("brave", Assert.Single(routing.Sessions).ProcessName);
    }

    // ---------------- Writes ----------------

    [Fact]
    public async Task RouteAppAsync_LowLevel_BuildsVerifiedEscapedRoute()
    {
        var transport = new FakeTransport();
        var manager = new AppRoutingManager(transport);

        await manager.RouteAppAsync(7244, "{0.0.0.00000000}.{6587c8e4}", AudioDataFlow.Render);

        // Route shape verified live on 2026-08-26; braces must be URL-escaped like the official UI does
        string route = Assert.Single(transport.PutRoutes);
        Assert.StartsWith("AudioDeviceRouting/render/", route);
        Assert.EndsWith("/7244", route);
        Assert.DoesNotContain("{", route);
    }

    [Fact]
    public async Task RouteAppAsync_ByChannel_ResolvesDeviceAtCallTime()
    {
        // The high-level overload must GET the routing state and target the channel's device
        var transport = new FakeTransport().With(SonarRoutes.AudioDeviceRouting, RoutingDump);
        var manager = new AppRoutingManager(transport);

        await manager.RouteAppAsync(7244, Channel.Media);

        string route = Assert.Single(transport.PutRoutes);
        Assert.Contains("6587c8e4", route); // the Media VAD from the fixture
    }

    [Fact]
    public async Task RouteAppAsync_UnknownChannelDevice_ThrowsSonarResponse()
    {
        // Aux has no device in this fixture: the resolution must fail loudly, not silently no-op
        var transport = new FakeTransport().With(SonarRoutes.AudioDeviceRouting, RoutingDump);
        var manager = new AppRoutingManager(transport);

        await Assert.ThrowsAsync<SonarResponseException>(
            () => manager.RouteAppAsync(7244, Channel.Aux));
        Assert.Empty(transport.PutRoutes);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-42)]
    public async Task RouteAppAsync_InvalidProcessId_ThrowsWithoutSending(int invalidPid)
    {
        var manager = new AppRoutingManager(new FakeTransport());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => manager.RouteAppAsync(invalidPid, "device", AudioDataFlow.Render));
    }

    private static JsonElement Json(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }
}