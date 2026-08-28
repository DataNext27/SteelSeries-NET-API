using SteelSeriesAPI.Core;
using SteelSeriesAPI.Sonar;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Managers;
using SteelSeriesAPI.Sonar.Models;
using Xunit;

namespace SteelSeriesAPI.Tests;

public class VolumeSettingsManagerTests
{
    // Real dump captured on 2026-08-07 (streamer mode active, classic route)
    private const string ClassicDump =
        """
        {"masters":{"stream":{},"classic":{"volume":0.0,"muted":false}},"devices":{"game":{"stream":{},"classic":{"volume":1.0,"muted":false}},"chatRender":{"stream":{},"classic":{"volume":1.0,"muted":false}},"chatCapture":{"stream":{},"classic":{"volume":1.0,"muted":false}},"media":{"stream":{},"classic":{"volume":1.0,"muted":false}},"aux":{"stream":{},"classic":{"volume":1.0,"muted":false}}}}
        """;

    // Real dump captured on 2026-08-07 (streamer mode active, streamer route)
    private const string StreamerDump =
        """
        {"masters":{"stream":{"streaming":{"volume":1.0,"muted":false},"monitoring":{"volume":1.0,"muted":false}},"classic":{"volume":0.0,"muted":false}},"devices":{"game":{"stream":{"streaming":{"volume":1.0,"muted":false},"monitoring":{"volume":0.42,"muted":false}},"classic":{"volume":0.0,"muted":false}},"chatRender":{"stream":{"streaming":{"volume":1.0,"muted":false},"monitoring":{"volume":1.0,"muted":false}},"classic":{"volume":0.0,"muted":false}},"chatCapture":{"stream":{"streaming":{"volume":1.0,"muted":false},"monitoring":{"volume":1.0,"muted":false}},"classic":{"volume":0.0,"muted":false}},"media":{"stream":{"streaming":{"volume":1.0,"muted":false},"monitoring":{"volume":1.0,"muted":false}},"classic":{"volume":0.0,"muted":false}},"aux":{"stream":{"streaming":{"volume":1.0,"muted":false},"monitoring":{"volume":1.0,"muted":false}},"classic":{"volume":0.0,"muted":false}}}}
        """;

    [Fact]
    public async Task GetAsync_ClassicMaster_ParsesVolumeAndMute()
    {
        var transport = new FakeTransport().With(SonarRoutes.ClassicVolumes, ClassicDump);
        var manager = new VolumeSettingsManager(transport);

        var setting = await manager.GetAsync(Channel.Master);

        Assert.Equal(new VolumeSetting(0.0, false), setting);
    }

    [Fact]
    public async Task GetAsync_ClassicGame_ParsesDeviceChannel()
    {
        var transport = new FakeTransport().With(SonarRoutes.ClassicVolumes, ClassicDump);
        var manager = new VolumeSettingsManager(transport);

        var setting = await manager.GetAsync(Channel.Game);

        Assert.Equal(new VolumeSetting(1.0, false), setting);
    }

    [Fact]
    public async Task GetAsync_StreamerPersonalMix_ReadsMonitoringVolume()
    {
        var transport = new FakeTransport().With(SonarRoutes.StreamerVolumes, StreamerDump);
        var manager = new VolumeSettingsManager(transport);

        var setting = await manager.GetAsync(Channel.Game, Mix.Personal);

        Assert.Equal(new VolumeSetting(0.42, false), setting);
    }

    [Fact]
    public async Task SetVolumeAsync_BuildsInvariantCultureRoute()
    {
        // Guards against "0,37" appearing in URLs on French/German machines
        var transport = new FakeTransport();
        var manager = new VolumeSettingsManager(transport);

        await manager.SetVolumeAsync(Channel.Game, 0.37);

        Assert.Equal("volumeSettings/classic/game/Volume/0.37", Assert.Single(transport.PutRoutes));
    }

    [Fact]
    public async Task SetMuteAsync_BuildsLowercaseBooleanRoute()
    {
        var transport = new FakeTransport();
        var manager = new VolumeSettingsManager(transport);

        await manager.SetMuteAsync(Channel.Chat, true);

        Assert.Equal("volumeSettings/classic/chatRender/Mute/true", Assert.Single(transport.PutRoutes));
    }

    [Theory]
    [InlineData(1.5)]
    [InlineData(double.NaN)]
    public async Task SetVolumeAsync_OutOfRange_Throws(double invalid)
    {
        var transport = new FakeTransport();
        var manager = new VolumeSettingsManager(transport);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => manager.SetVolumeAsync(Channel.Game, invalid));
        Assert.Empty(transport.PutRoutes); // validation must happen BEFORE any HTTP call
    }

    [Fact]
    public async Task GetAsync_EmptyStreamNode_ThrowsSonarResponse()
    {
        // In the classic dump, "stream" nodes are empty objects: digging into a mix must fail loudly
        var transport = new FakeTransport().With(SonarRoutes.StreamerVolumes, ClassicDump);
        var manager = new VolumeSettingsManager(transport);

        await Assert.ThrowsAsync<SonarResponseException>(
            () => manager.GetAsync(Channel.Game, Mix.Personal));
    }
}