namespace SteelSeriesAPI.Sonar.Events;

/// <summary>
/// Central registry of every event name broadcast on the Sonar WebSocket (/sock).
/// Full catalog extracted from the GG UI bundle on 2026-08-08.
/// Note the inconsistent EVENT_SONAR_ vs SONAR_EVENT_ prefixes: that is SteelSeries' doing.
/// </summary>
internal static class SonarEventNames
{
    internal const string ChatMixData = "EVENT_SONAR_CHATMIX_DATA";
    internal const string VolumeData = "SONAR_EVENT_VOLUME_DATA";
    internal const string RedirectionStatusUpdate = "SONAR_EVENT_REDIRECTION_STATUS_UPDATE";
    internal const string DeviceStatusUpdate = "SONAR_EVENT_DEVICE_STATUS_UPDATE";
    internal const string DeviceDefaultUpdate = "SONAR_EVENT_DEVICE_DEFAULT_UPDATE";
    internal const string AudioSessionOpened = "SONAR_EVENT_AUDIO_SESSION_OPENED_DATA";
    internal const string AudioSessionClosed = "SONAR_EVENT_AUDIO_SESSION_CLOSED_DATA";
    internal const string SelectedConfigUpdated = "SONAR_EVENT_SELECTED_CONFIG_UPDATED";
    internal const string StreamMonitoringLockStatusUpdate = "SONAR_EVENT_STREAM_MONITORING_LOCK_STATUS_UPDATE";

    // Known to exist (UI bundle catalog) but not yet wired to typed events:
    // EVENT_SONAR_STATUS, SONAR_EVENT_DEVICE_OUT_VOLUME_DATA, SONAR_EVENT_DEVICE_VOLUMES_UPDATE,
    // SONAR_EVENT_FALLBACK_UPDATED, SONAR_EVENT_FAVORITE_CONFIGS_UPDATED, SONAR_EVENT_FEATURE_UPDATED,
    // SONAR_EVENT_PLAYER_STOPPED, SONAR_EVENT_QUICKSET_*, SONAR_EVENT_RECORDING_STOPPED,
    // SONAR_EVENT_ROUTING_DATA
}