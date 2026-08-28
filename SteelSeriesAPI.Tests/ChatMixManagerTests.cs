using SteelSeriesAPI.Core;
using SteelSeriesAPI.Sonar;
using SteelSeriesAPI.Sonar.Managers;
using SteelSeriesAPI.Sonar.Models;
using Xunit;

namespace SteelSeriesAPI.Tests;

public class ChatMixManagerTests
{
    // Real responses captured on 2026-08-08
    private const string EnabledDump =
        """{"balance":0.45999998,"state":"enabled","id":"00000000-0000-0000-0000-000000000000"}""";

    private const string DifferentDeviceDump =
        """{"balance":0.0,"state":"differentDeviceSelected","id":"00000000-0000-0000-0000-000000000000"}""";

    [Fact]
    public async Task GetAsync_EnabledState_ParsesBalanceAndState()
    {
        var transport = new FakeTransport().With(SonarRoutes.GetChatMix, EnabledDump);
        var manager = new ChatMixManager(transport);

        var setting = await manager.GetAsync();

        Assert.Equal(new ChatMixSetting(0.45999998, "enabled"), setting);
    }

    [Fact]
    public async Task GetAsync_DifferentDeviceSelected_ParsesState()
    {
        var transport = new FakeTransport().With(SonarRoutes.GetChatMix, DifferentDeviceDump);
        var manager = new ChatMixManager(transport);

        var setting = await manager.GetAsync();

        Assert.Equal(new ChatMixSetting(0.0, "differentDeviceSelected"), setting);
    }

    [Fact]
    public async Task GetAsync_UnknownFieldsAndMissingState_StillWorks()
    {
        // A future GG update may drop/rename fields around the ones we read:
        // parsing must degrade gracefully, never crash.
        var transport = new FakeTransport().With(SonarRoutes.GetChatMix,
            """{"balance":0.5,"someNewField":true}""");
        var manager = new ChatMixManager(transport);

        var setting = await manager.GetAsync();

        Assert.Equal(new ChatMixSetting(0.5, null), setting);
    }

    [Fact]
    public async Task SetAsync_NegativeBalance_BuildsInvariantCultureRoute()
    {
        // Guards both the invariant decimal separator and the negative sign handling
        var transport = new FakeTransport();
        var manager = new ChatMixManager(transport);

        await manager.SetAsync(-0.5);

        Assert.Equal("v1/chatMix?balance=-0.50", Assert.Single(transport.PutRoutes));
    }

    [Theory]
    [InlineData(-1.5)]
    [InlineData(1.5)]
    [InlineData(double.NaN)]
    public async Task SetAsync_OutOfRange_ThrowsWithoutSendingAnything(double invalid)
    {
        var transport = new FakeTransport();
        var manager = new ChatMixManager(transport);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => manager.SetAsync(invalid));
        Assert.Empty(transport.PutRoutes); // validation must happen BEFORE any HTTP call
    }
}