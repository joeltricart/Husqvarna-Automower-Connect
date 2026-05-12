namespace HusqvarnaAutomowerConnect.Core.Models;

public sealed record MowerSchedule
{
    public IReadOnlyList<ScheduleTask> Tasks { get; init; } = [];

    public DateTimeOffset? UpdatedAt { get; init; }
}

public sealed record ScheduleTask
{
    public string? Id { get; init; }

    public IReadOnlySet<DayOfWeek> Days { get; init; } = new HashSet<DayOfWeek>();

    public required TimeOnly StartTime { get; init; }

    public required TimeSpan Duration { get; init; }

    public string? WorkAreaId { get; init; }

    public bool? Enabled { get; init; }
}

