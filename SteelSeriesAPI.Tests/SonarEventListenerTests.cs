using System.Text.Json;
using SteelSeriesAPI.Sonar.Enums;
using SteelSeriesAPI.Sonar.Events;
using SteelSeriesAPI.Sonar.Models;
using Xunit;

namespace SteelSeriesAPI.Tests;

public class SonarEventListenerTests
{
    // ---------------------------------------------------------------
    // Fixtures: real payloads captured from ws://.../sock on 2026-08-08
    // (the "data" part of each event message, trimmed to 3 channels
    // where the full capture had 6 - the parser does not require all)
    // ---------------------------------------------------------------

    private const string ChatMixData =
        """{"balance":-0.53,"state":"enabled","id":"00000000-0000-0000-0000-000000000000"}""";

    // Streamer mode active: "stream" sections filled, "classic" sections stale
    private const string StreamerVolumeData =
        """
        {"masters":{"stream":{"streaming":{"volume":1.0,"muted":false},"monitoring":{"volume":1.0,"muted":false}},"classic":{"volume":0.0,"muted":false}},
         "devices":{
           "game":{"stream":{"streaming":{"volume":1.0,"muted":false},"monitoring":{"volume":0.42,"muted":false}},"classic":{"volume":0.0,"muted":false}},
           "chatRender":{"stream":{"streaming":{"volume":0.33,"muted":true},"monitoring":{"volume":1.0,"muted":false}},"classic":{"volume":0.0,"muted":false}}}}
        """;

    // Classic mode active: "stream" sections are empty objects
    private const string ClassicVolumeData =
        """
        {"masters":{"stream":{},"classic":{"volume":1.0,"muted":false}},
         "devices":{
           "game":{"stream":{},"classic":{"volume":0.5,"muted":false}},
           "media":{"stream":{},"classic":{"volume":1.0,"muted":true}}}}
        """;

    /// <summary>Parses a JSON string and returns a JsonElement detached from the document lifetime.</summary>
    private static JsonElement Json(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    /// <summary>Shortcut to build a ChannelVolumes with optional per-mix settings.</summary>
    private static ChannelVolumes Volumes(
        (double vol, bool muted)? classic = null,
        (double vol, bool muted)? personal = null,
        (double vol, bool muted)? stream = null)
    {
        return new ChannelVolumes(
            classic is { } c ? new VolumeSetting(c.vol, c.muted) : null,
            personal is { } p ? new VolumeSetting(p.vol, p.muted) : null,
            stream is { } s ? new VolumeSetting(s.vol, s.muted) : null);
    }

    // ---------------------------------------------------------------
    // ParseChatMix
    // ---------------------------------------------------------------

    [Fact]
    public void ParseChatMix_RealPayload_ParsesBalanceAndState()
    {
        var setting = SonarEventListener.ParseChatMix(Json(ChatMixData));

        Assert.Equal(new ChatMixSetting(-0.53, "enabled"), setting);
    }

    [Fact]
    public void ParseChatMix_NullData_ReturnsNeutralDefaults()
    {
        // Some Sonar events arrive with "data": null - parsing must never crash
        var setting = SonarEventListener.ParseChatMix(Json("null"));

        Assert.Equal(new ChatMixSetting(0.0, null), setting);
    }

    // ---------------------------------------------------------------
    // ParseVolumeSnapshot
    // ---------------------------------------------------------------

    [Fact]
    public void ParseVolumeSnapshot_StreamerCapture_ParsesAllMixes()
    {
        var snapshot = SonarEventListener.ParseVolumeSnapshot(Json(StreamerVolumeData));

        Assert.Equal(3, snapshot.Channels.Count); // Master + game + chatRender

        var game = snapshot.Channels[Channel.Game];
        Assert.Equal(new VolumeSetting(0.42, false), game.Personal);
        Assert.Equal(new VolumeSetting(1.0, false), game.Stream);
        Assert.Equal(new VolumeSetting(0.0, false), game.Classic);

        var chat = snapshot.Channels[Channel.Chat];
        Assert.Equal(new VolumeSetting(0.33, true), chat.Stream);
    }

    [Fact]
    public void ParseVolumeSnapshot_ClassicCapture_MixesAreNull()
    {
        // In classic mode the "stream" nodes are empty objects: mixes must be null, not zeroed
        var snapshot = SonarEventListener.ParseVolumeSnapshot(Json(ClassicVolumeData));

        var game = snapshot.Channels[Channel.Game];
        Assert.Equal(new VolumeSetting(0.5, false), game.Classic);
        Assert.Null(game.Personal);
        Assert.Null(game.Stream);

        Assert.Equal(new VolumeSetting(1.0, true), snapshot.Channels[Channel.Media].Classic);
    }

    [Fact]
    public void ParseVolumeSnapshot_UnknownChannel_IsSkippedWithoutCrashing()
    {
        // A future GG update adding a new channel must not break parsing (the V1 lesson)
        const string withUnknown =
            """
            {"masters":{"stream":{},"classic":{"volume":1.0,"muted":false}},
             "devices":{
               "subwoofer":{"stream":{},"classic":{"volume":0.8,"muted":false}},
               "game":{"stream":{},"classic":{"volume":0.5,"muted":false}}}}
            """;

        var snapshot = SonarEventListener.ParseVolumeSnapshot(Json(withUnknown));

        Assert.Equal(2, snapshot.Channels.Count); // Master + game, subwoofer skipped
        Assert.True(snapshot.Channels.ContainsKey(Channel.Game));
    }

    // ---------------------------------------------------------------
    // Diff
    // ---------------------------------------------------------------

    [Fact]
    public void Diff_ClassicVolumeChange_IsDetected()
    {
        var previous = new VolumeSnapshot(new Dictionary<Channel, ChannelVolumes>
        {
            [Channel.Game] = Volumes(classic: (0.5, false))
        });
        var current = new VolumeSnapshot(new Dictionary<Channel, ChannelVolumes>
        {
            [Channel.Game] = Volumes(classic: (0.8, false))
        });

        var change = Assert.Single(SonarEventListener.Diff(previous, current, Mode.Classic));

        Assert.Equal(Channel.Game, change.Channel);
        Assert.Null(change.Mix);
        Assert.Equal(0.8, change.NewVolume);
        Assert.Equal(0.5, change.PreviousVolume);
        Assert.False(change.MuteToggled);
    }

    [Fact]
    public void Diff_MuteToggle_SetsMuteToggled()
    {
        var previous = new VolumeSnapshot(new Dictionary<Channel, ChannelVolumes>
        {
            [Channel.Chat] = Volumes(classic: (0.4, false))
        });
        var current = new VolumeSnapshot(new Dictionary<Channel, ChannelVolumes>
        {
            [Channel.Chat] = Volumes(classic: (0.4, true))
        });

        var change = Assert.Single(SonarEventListener.Diff(previous, current, Mode.Classic));

        Assert.True(change.MuteToggled);
        Assert.True(change.IsMuted);
        Assert.False(change.WasMuted);
    }

    [Fact]
    public void Diff_InClassicMode_IgnoresStreamerSections()
    {
        // The other mode's sections return stale data (observed 2026-08-08):
        // a Personal change must NOT produce an event while diffing in classic mode.
        var previous = new VolumeSnapshot(new Dictionary<Channel, ChannelVolumes>
        {
            [Channel.Game] = Volumes(classic: (0.5, false), personal: (1.0, false))
        });
        var current = new VolumeSnapshot(new Dictionary<Channel, ChannelVolumes>
        {
            [Channel.Game] = Volumes(classic: (0.5, false), personal: (0.2, false))
        });

        Assert.Empty(SonarEventListener.Diff(previous, current, Mode.Classic));
    }

    [Fact]
    public void Diff_InStreamerMode_DetectsBothMixesIndependently()
    {
        var previous = new VolumeSnapshot(new Dictionary<Channel, ChannelVolumes>
        {
            [Channel.Game] = Volumes(personal: (1.0, false), stream: (1.0, false))
        });
        var current = new VolumeSnapshot(new Dictionary<Channel, ChannelVolumes>
        {
            [Channel.Game] = Volumes(personal: (0.7, false), stream: (0.3, false))
        });

        var changes = SonarEventListener.Diff(previous, current, Mode.Streamer).ToList();

        Assert.Equal(2, changes.Count);
        Assert.Contains(changes, c => c.Mix == Mix.Personal && c.NewVolume == 0.7);
        Assert.Contains(changes, c => c.Mix == Mix.Stream && c.NewVolume == 0.3);
    }

    [Fact]
    public void Diff_IdenticalSnapshots_ReturnsNothing()
    {
        var snapshot = new VolumeSnapshot(new Dictionary<Channel, ChannelVolumes>
        {
            [Channel.Game] = Volumes(classic: (0.5, false)),
            [Channel.Media] = Volumes(classic: (1.0, true))
        });

        Assert.Empty(SonarEventListener.Diff(snapshot, snapshot, Mode.Classic));
    }

    [Fact]
    public void Diff_ChannelAbsentFromPrevious_IsSkipped()
    {
        // First sighting of a channel: nothing to compare against, no event
        var previous = new VolumeSnapshot(new Dictionary<Channel, ChannelVolumes>());
        var current = new VolumeSnapshot(new Dictionary<Channel, ChannelVolumes>
        {
            [Channel.Game] = Volumes(classic: (0.5, false))
        });

        Assert.Empty(SonarEventListener.Diff(previous, current, Mode.Classic));
    }

    // ----------------------------------------------------------------
    // Redirections diff
    // ----------------------------------------------------------------

    [Fact]
    public void DiffRedirections_MicDeviceChange_IsDetected()
    {
        var previous = new SonarEventListener.RedirectionsSnapshot(
            [],
            new StreamRedirections(null, null, new MicRedirection("{cap-1}", true)),
            MonitoringEnabled: false);
        var current = previous with
        {
            Stream = new StreamRedirections(null, null, new MicRedirection("{cap-2}", true)),
        };

        var diff = SonarEventListener.DiffRedirections(previous, current);

        Assert.NotNull(diff.MicDeviceChange);
        Assert.Equal("{cap-1}", diff.MicDeviceChange!.PreviousDeviceId);
        Assert.Equal("{cap-2}", diff.MicDeviceChange.NewDeviceId);
        Assert.False(diff.IsEmpty);
    }

    [Fact]
    public void DiffRedirections_MicAbsentOrUnchanged_YieldsNoMicChange()
    {
        var withMic = new SonarEventListener.RedirectionsSnapshot(
            [],
            new StreamRedirections(null, null, new MicRedirection("{cap-1}", true)),
            MonitoringEnabled: false);
        var withoutMic = withMic with { Stream = new StreamRedirections(null, null, null) };

        Assert.Null(SonarEventListener.DiffRedirections(withMic, withMic).MicDeviceChange);
        Assert.Null(SonarEventListener.DiffRedirections(withoutMic, withMic).MicDeviceChange);
        Assert.Null(SonarEventListener.DiffRedirections(withMic, withoutMic).MicDeviceChange);
    }
}