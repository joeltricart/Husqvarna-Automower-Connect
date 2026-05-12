namespace HusqvarnaAutomowerConnect.Core.Models;

public enum MowerCommandType
{
    Pause = 0,
    ParkUntilNextSchedule = 1,
    ParkUntilFurtherNotice = 2,
    ResumeSchedule = 3,
    StartForDuration = 4
}

public sealed record MowerCommand
{
    public required MowerCommandType Type { get; init; }

    public TimeSpan? Duration { get; init; }

    public string? WorkAreaId { get; init; }
}

