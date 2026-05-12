namespace HusqvarnaAutomowerConnect.Core.Models;

public enum MowerState
{
    Unknown = 0,
    Ready = 1,
    InOperation = 2,
    Paused = 3,
    Parked = 4,
    Error = 5,
    Offline = 6
}

public enum MowerActivity
{
    Unknown = 0,
    Mowing = 1,
    GoingHome = 2,
    Charging = 3,
    Leaving = 4,
    ParkedInChargingStation = 5,
    Stopped = 6
}

public enum MowerMode
{
    Unknown = 0,
    MainArea = 1,
    SecondaryArea = 2,
    Home = 3,
    Demo = 4
}

public sealed record MowerStatus
{
    public MowerState State { get; init; } = MowerState.Unknown;

    public MowerActivity Activity { get; init; } = MowerActivity.Unknown;

    public MowerMode Mode { get; init; } = MowerMode.Unknown;

    public bool? Connected { get; init; }

    public string? RestrictedReason { get; init; }

    public MowerError? Error { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }
}

