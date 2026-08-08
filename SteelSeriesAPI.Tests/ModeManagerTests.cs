using SteelSeriesAPI.Sonar;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Managers;
using SteelSeriesAPI.Core;
using Xunit;

namespace SteelSeriesAPI.Tests;

public class ModeManagerTests
{
    [Fact]
    public async Task GetAsync_BareJsonString_ParsesMode()
    {
        // The mode route returns a bare JSON string, not an object (observed 2026-08-07)
        var transport = new FakeTransport().With(SonarRoutes.GetMode, "\"stream\"");
        var manager = new ModeManager(transport);

        Assert.Equal(Mode.Streamer, await manager.GetAsync());
    }

    [Fact]
    public async Task GetAsync_UnknownValue_ThrowsSonarResponse()
    {
        var transport = new FakeTransport().With(SonarRoutes.GetMode, "\"quantum\"");
        var manager = new ModeManager(transport);

        await Assert.ThrowsAsync<SonarResponseException>(() => manager.GetAsync());
    }

    [Fact]
    public async Task SetAsync_ConfirmedByReadback_Succeeds()
    {
        // The fake always reports "stream": switching to Streamer confirms on first poll
        var transport = new FakeTransport().With(SonarRoutes.GetMode, "\"stream\"");
        var manager = new ModeManager(transport);

        await manager.SetAsync(Mode.Streamer);

        Assert.Equal("mode/stream", Assert.Single(transport.PutRoutes));
    }
}