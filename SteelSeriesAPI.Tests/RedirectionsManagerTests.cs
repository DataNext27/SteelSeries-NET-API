using SteelSeriesAPI.Core;
using SteelSeriesAPI.Sonar;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Managers;
using SteelSeriesAPI.Sonar.Models;
using Xunit;

namespace SteelSeriesAPI.Tests;

public class RedirectionsManagerTests
{
    // Real dumps captured on 2026-08-14 (streamer mode active)
    private const string ClassicDump =
        """
        [
          {"id":"aux","deviceId":"{0.0.0.00000000}.{683a5c4e-d3ff-46b0-ab5a-96b5b8d4b11e}","isRunning":true},
          {"id":"chat","deviceId":"{0.0.0.00000000}.{584e246f-4ae6-417c-acbb-c9f90fee29f1}","isRunning":true},
          {"id":"game","deviceId":"{0.0.0.00000000}.{584e246f-4ae6-417c-acbb-c9f90fee29f1}","isRunning":true},
          {"id":"media","deviceId":"{0.0.0.00000000}.{683a5c4e-d3ff-46b0-ab5a-96b5b8d4b11e}","isRunning":true},
          {"id":"mic","deviceId":"{0.0.1.00000000}.{28da6e04-ea4b-4391-b05c-e505e5c60d50}","isRunning":true}
        ]
        """;

    private const string StreamDump =
        """
        [
          {"streamRedirectionId":"streaming","deviceId":"{0.0.0.00000000}.{bd318855-e5cc-48cf-b878-3b5dc40d81a9}",
           "status":[{"role":"chatCapture","isEnabled":true},{"role":"chatRender","isEnabled":false},
                     {"role":"game","isEnabled":false},{"role":"media","isEnabled":false},{"role":"aux","isEnabled":true}],
           "isRunning":false},
          {"streamRedirectionId":"monitoring","deviceId":"{0.0.0.00000000}.{584e246f-4ae6-417c-acbb-c9f90fee29f1}",
           "status":[{"role":"chatCapture","isEnabled":false},{"role":"chatRender","isEnabled":true},
                     {"role":"game","isEnabled":true},{"role":"media","isEnabled":true},{"role":"aux","isEnabled":true}],
           "isRunning":true},
          {"streamRedirectionId":"mic","deviceId":"{0.0.1.00000000}.{28da6e04-ea4b-4391-b05c-e505e5c60d50}",
           "status":[],"isRunning":true}
        ]
        """;

    // ---------------- Parsing ----------------

    [Fact]
    public async Task GetClassicRedirectionsAsync_RealDump_ParsesAllChannels()
    {
        var transport = new FakeTransport().With(SonarRoutes.ClassicRedirections, ClassicDump);
        var manager = new RedirectionsManager(transport);

        var redirections = await manager.GetClassicRedirectionsAsync();

        Assert.Equal(5, redirections.Count);

        // "chat" and "mic" short ids must map to the Chat and Mic channels
        var chat = Assert.Single(redirections, r => r.Channel == Channel.Chat);
        Assert.Equal("{0.0.0.00000000}.{584e246f-4ae6-417c-acbb-c9f90fee29f1}", chat.DeviceId);
        Assert.True(chat.IsRunning);

        Assert.Single(redirections, r => r.Channel == Channel.Mic);
        Assert.DoesNotContain(redirections, r => r.Channel == Channel.Master);
    }

    [Fact]
    public async Task GetStreamRedirectionsAsync_RealDump_ParsesMixesAndMic()
    {
        var transport = new FakeTransport().With(SonarRoutes.StreamRedirections, StreamDump);
        var manager = new RedirectionsManager(transport);

        var state = await manager.GetStreamRedirectionsAsync();

        // "monitoring" -> Personal, "streaming" -> Stream
        Assert.NotNull(state.Personal);
        Assert.True(state.Personal!.IsRunning);
        Assert.True(state.Personal.EnabledChannels[Channel.Game]);
        Assert.False(state.Personal.EnabledChannels[Channel.Mic]);   // chatCapture: false

        Assert.NotNull(state.Stream);
        Assert.False(state.Stream!.IsRunning);
        Assert.False(state.Stream.EnabledChannels[Channel.Chat]);    // chatRender: false
        Assert.True(state.Stream.EnabledChannels[Channel.Aux]);

        Assert.NotNull(state.Mic);
        Assert.True(state.Mic!.IsRunning);
    }

    [Fact]
    public void ParseStreamRedirections_UnknownRedirectionId_IsSkipped()
    {
        const string withUnknown =
            """
            [{"streamRedirectionId":"holographic","deviceId":"x","status":[],"isRunning":true},
             {"streamRedirectionId":"monitoring","deviceId":"y","status":[],"isRunning":true}]
            """;

        var state = RedirectionsManager.ParseStreamRedirections(Json(withUnknown));

        Assert.NotNull(state.Personal);
        Assert.Null(state.Stream);
    }

    // ---------------- Write routes ----------------

    [Fact]
    public async Task SetStreamMonitoringEnabledAsync_BuildsVerifiedRoute()
    {
        var transport = new FakeTransport();
        var manager = new RedirectionsManager(transport);

        await manager.SetStreamMonitoringEnabledAsync(true);

        // Route verified against the live API on 2026-08-14
        Assert.Equal("streamRedirections/isStreamMonitoringEnabled/true", Assert.Single(transport.PutRoutes));
    }

    [Fact]
    public async Task SetMixChannelEnabledAsync_UsesJsonVocabulary()
    {
        var transport = new FakeTransport();
        var manager = new RedirectionsManager(transport);

        await manager.SetMixChannelEnabledAsync(Mix.Personal, Channel.Mic, false);

        // Stream redirection routes use "chatCapture", not "mic" (verified 2026-08-14)
        Assert.Equal("streamRedirections/monitoring/redirections/chatCapture/isEnabled/false",
            Assert.Single(transport.PutRoutes));
    }

    [Fact]
    public async Task SetMicDeviceAsync_UsesTheMicStreamRedirectionRoute()
    {
        var transport = new FakeTransport();
        var manager = new RedirectionsManager(transport);

        await manager.SetMicDeviceAsync("{0.0.1}.{abc}");

        string route = Assert.Single(transport.PutRoutes);
        Assert.StartsWith("streamRedirections/mic/deviceId/", route);
        Assert.DoesNotContain("{", route); // braces escaped
    }

    [Fact]
    public async Task SetClassicDeviceAsync_UsesShortVocabularyAndEscapesDeviceId()
    {
        var transport = new FakeTransport();
        var manager = new RedirectionsManager(transport);

        await manager.SetClassicDeviceAsync(Channel.Mic, "{0.0.1}.{abc}");

        string route = Assert.Single(transport.PutRoutes);
        Assert.StartsWith("classicRedirections/mic/deviceId/", route);   // short id "mic"
        Assert.DoesNotContain("{", route);                               // braces escaped
    }

    [Fact]
    public async Task SetMixChannelEnabledAsync_Master_Throws()
    {
        var manager = new RedirectionsManager(new FakeTransport());

        await Assert.ThrowsAsync<ArgumentException>(
            () => manager.SetMixChannelEnabledAsync(Mix.Personal, Channel.Master, true));
    }

    private static System.Text.Json.JsonElement Json(string json)
    {
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }
}