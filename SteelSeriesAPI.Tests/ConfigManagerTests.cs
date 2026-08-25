using System.Text.Json;
using SteelSeriesAPI.Core;
using SteelSeriesAPI.Sonar;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Managers;
using Xunit;

namespace SteelSeriesAPI.Tests;

public class ConfigManagerTests
{
    // Shape from real dumps (2026-08-25, GG 118 / Sonar schema v5), EQ payloads trimmed:
    // the parser must ignore data/defaultData whatever they contain.
    private const string ConfigsDump =
        """
        [
          {
            "id": "e6979db3-3e00-4399-b58c-6f026c9ef6ba",
            "name": "Custom",
            "createdAt": "2026-08-04T02:45:38",
            "updatedAt": "2026-08-10T19:34:39.4703773",
            "virtualAudioDevice": "game",
            "data": { "parametricEQ": { "filter1": { "gain": 4.0 } } },
            "schemaVersion": 5,
            "isPreset": false,
            "defaultData": { "whatever": true },
            "image": "sonar.svg",
            "isFavorite": true,
            "favoritePosition": 0,
            "releaseVersion": null
          },
          {
            "id": "ed310e25-2f10-49bb-bb74-5978a44bb9be",
            "name": "Halo: Campaign Evolved",
            "virtualAudioDevice": "game",
            "data": {},
            "isPreset": true,
            "isFavorite": false,
            "releaseVersion": "1.100.0"
          },
          {
            "id": "aaaa1111-2222-3333-4444-555566667777",
            "name": "Mic Custom",
            "virtualAudioDevice": "chatCapture",
            "data": {},
            "isPreset": false,
            "isFavorite": false
          }
        ]
        """;

    private const string SelectedDump =
        """
        [
          { "id": "e6979db3-3e00-4399-b58c-6f026c9ef6ba", "name": "Custom", "virtualAudioDevice": "game", "isPreset": false, "isFavorite": true },
          { "id": "aaaa1111-2222-3333-4444-555566667777", "name": "Mic Custom", "virtualAudioDevice": "chatCapture", "isPreset": false, "isFavorite": false },
          { "id": "bbbb1111-2222-3333-4444-555566667777", "name": "Flat", "virtualAudioDevice": "media", "isPreset": true, "isFavorite": false }
        ]
        """;

    // ---------------- Listing ----------------

    [Fact]
    public async Task GetAllAsync_RealShape_ParsesHeadersAndIgnoresPayloads()
    {
        var transport = new FakeTransport().With(SonarRoutes.Configs, ConfigsDump);
        var manager = new ConfigManager(transport);

        var configs = await manager.GetAllAsync();

        Assert.Equal(3, configs.Count);

        var custom = Assert.Single(configs, c => c.Name == "Custom");
        Assert.Equal("e6979db3-3e00-4399-b58c-6f026c9ef6ba", custom.Id);
        Assert.Equal(Channel.Game, custom.Channel);
        Assert.False(custom.IsPreset);
        Assert.True(custom.IsFavorite);

        // "chatCapture" virtualAudioDevice must map to the Mic channel
        Assert.Single(configs, c => c.Channel == Channel.Mic);
    }

    [Fact]
    public async Task GetAllAsync_ByChannel_Filters()
    {
        var transport = new FakeTransport().With(SonarRoutes.Configs, ConfigsDump);
        var manager = new ConfigManager(transport);

        var gameConfigs = await manager.GetAllAsync(Channel.Game);

        Assert.Equal(2, gameConfigs.Count);
        Assert.All(gameConfigs, c => Assert.Equal(Channel.Game, c.Channel));
    }

    [Fact]
    public void ParseConfigList_UnknownDeviceOrMissingId_IsSkipped()
    {
        // A future GG update adding a new virtualAudioDevice, or a malformed entry,
        // must never break the listing (the V1 lesson).
        const string withOddities =
            """
            [
              { "id": "x", "name": "Subwoofer thing", "virtualAudioDevice": "subwoofer" },
              { "name": "No id at all", "virtualAudioDevice": "game" },
              { "id": "y", "name": "Valid", "virtualAudioDevice": "aux" }
            ]
            """;

        var configs = ConfigManager.ParseConfigList(Json(withOddities));

        var valid = Assert.Single(configs);
        Assert.Equal(Channel.Aux, valid.Channel);
    }

    // ---------------- Selection ----------------

    [Fact]
    public async Task GetSelectedAsync_ReturnsOneConfigPerChannel()
    {
        var transport = new FakeTransport().With(SonarRoutes.SelectedConfigs, SelectedDump);
        var manager = new ConfigManager(transport);

        var selected = await manager.GetSelectedAsync();

        Assert.Equal(3, selected.Count);
        Assert.Equal("Custom", selected[Channel.Game].Name);
        Assert.Equal("Mic Custom", selected[Channel.Mic].Name);
        Assert.Equal("Flat", selected[Channel.Media].Name);
    }

    [Fact]
    public async Task GetSelectedAsync_ChannelWithoutSelection_ReturnsNull()
    {
        var transport = new FakeTransport().With(SonarRoutes.SelectedConfigs, SelectedDump);
        var manager = new ConfigManager(transport);

        Assert.Null(await manager.GetSelectedAsync(Channel.Aux));
    }

    // ---------------- Select (write) ----------------

    [Fact]
    public async Task SelectAsync_BuildsVerifiedRoute()
    {
        var transport = new FakeTransport();
        var manager = new ConfigManager(transport);

        await manager.SelectAsync("ed310e25-2f10-49bb-bb74-5978a44bb9be");

        // Route verified against the live API on 2026-08-25
        Assert.Equal("configs/ed310e25-2f10-49bb-bb74-5978a44bb9be/select",
            Assert.Single(transport.PutRoutes));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SelectAsync_EmptyId_ThrowsWithoutSendingAnything(string invalid)
    {
        var transport = new FakeTransport();
        var manager = new ConfigManager(transport);

        await Assert.ThrowsAsync<ArgumentException>(() => manager.SelectAsync(invalid));
        Assert.Empty(transport.PutRoutes);
    }

    private static JsonElement Json(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }
}