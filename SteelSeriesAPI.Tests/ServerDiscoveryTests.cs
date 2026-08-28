using System.Text.Json;
using SteelSeriesAPI.Core;
using Xunit;

namespace SteelSeriesAPI.Tests;

public class ServerDiscoveryTests
{
    // Reduced fixture from the real /subApps dump (2026-08-04)
    private const string ValidSubApps =
        """
        {"subApps":{"sonar":{"name":"sonar","isEnabled":true,"toggleViaSettings":true,
        "autoStart":true,"isReady":true,"isRunning":true,"exitCode":0,
        "metadata":{"encryptedWebServerAddress":"","webServerAddress":"http://127.0.0.1:53784",
        "offlineFrontendAddress":"","onlineFrontendAddress":""},"secretMetadata":null}}}
        """;

    private const string SonarStopped =
        """{"subApps":{"sonar":{"isRunning":false,"metadata":null}}}""";

    // Regression: transient state observed on 2026-08-04 when GG restarts
    private const string SonarStartingUp =
        """{"subApps":{"sonar":{"isRunning":true,"metadata":null}}}""";

    private const string NoSonarEntry =
        """{"subApps":{"engine":{"isRunning":true}}}""";

    [Fact]
    public void ParseSonarAddress_WithValidPayload_ReturnsAddress()
    {
        var result = ServerDiscovery.ParseSonarAddress(ValidSubApps);

        Assert.Equal(new Uri("http://127.0.0.1:53784"), result);
    }

    [Fact]
    public void ParseSonarAddress_WhenSonarStopped_ThrowsSonarNotRunning()
    {
        Assert.Throws<SonarNotRunningException>(
            () => ServerDiscovery.ParseSonarAddress(SonarStopped));
    }

    [Fact]
    public void ParseSonarAddress_WhenSonarStartingUp_ThrowsSonarNotRunning()
    {
        // This case crashed with InvalidOperationException before the fix:
        // metadata is null for ~3s while Sonar is starting up.
        Assert.Throws<SonarNotRunningException>(
            () => ServerDiscovery.ParseSonarAddress(SonarStartingUp));
    }

    [Fact]
    public void ParseSonarAddress_WhenSonarEntryMissing_ThrowsDiscovery()
    {
        Assert.Throws<DiscoveryException>(
            () => ServerDiscovery.ParseSonarAddress(NoSonarEntry));
    }

    [Fact]
    public void ParseSonarAddress_WithUnknownExtraFields_StillWorks()
    {
        // Simulates a future GG update adding/renaming fields around the ones we read
        const string futureVersion =
            """
            {"subApps":{"sonar":{"isRunning":true,"someNewField":42,"renamedThing":"x",
            "metadata":{"webServerAddress":"http://127.0.0.1:9999","newMetaField":true}}}}
            """;

        var result = ServerDiscovery.ParseSonarAddress(futureVersion);

        Assert.Equal(new Uri("http://127.0.0.1:9999"), result);
    }
}