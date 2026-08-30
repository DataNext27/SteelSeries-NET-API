using SteelSeriesAPI.Sonar.Enums;

namespace SteelSeriesAPI.Sonar.Events;

/// <summary>
/// The Windows state of a physical audio device, as relayed by Sonar
/// (lowercase MMDevice states on the wire).
/// </summary>
public enum AudioDeviceState
{
    /// <summary>The device is present and usable.</summary>
    Active,
    /// <summary>The device is disabled in Windows.</summary>
    Disabled,
    /// <summary>The device is gone (unplugged USB, powered-off wireless...).</summary>
    NotPresent,
    /// <summary>The device is enabled but its jack is unplugged.</summary>
    Unplugged,
    /// <summary>A state this library does not know; see <see cref="AudioDeviceStatusChange.RawState"/>.</summary>
    Unknown
}

/// <summary>A physical audio device appeared, disappeared, or changed state.</summary>
/// <param name="DeviceId">The Windows device id.</param>
/// <param name="Name">The device friendly name.</param>
/// <param name="DataFlow">Whether this is an output (render) or input (capture) device.</param>
/// <param name="State">The new device state.</param>
/// <param name="RawState">The state string as sent by Sonar, kept verbatim for unknown values.</param>
public sealed record AudioDeviceStatusChange(
    string DeviceId,
    string Name,
    AudioDataFlow DataFlow,
    AudioDeviceState State,
    string RawState)
{
    /// <summary>True when the device is present and usable.</summary>
    public bool IsPresent => State == AudioDeviceState.Active;
}
