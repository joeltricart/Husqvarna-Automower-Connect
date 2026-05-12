namespace HusqvarnaAutomowerConnect.Core.Models;

public sealed record Mower
{
    public required string Id { get; init; }

    public string Name { get; init; } = "Robot sans nom";

    public string? Model { get; init; }

    public string? SerialNumber { get; init; }

    public MowerStatus? Status { get; init; }

    public BatteryInfo? Battery { get; init; }

    public MowerLocation? Location { get; init; }

    public MowerSchedule? Schedule { get; init; }

    public MowerCapabilities Capabilities { get; init; } = MowerCapabilities.None;

    public DateTimeOffset? LastUpdatedAt { get; init; }
}

public sealed record MowerCapabilities
{
    public static readonly MowerCapabilities None = new();

    public bool CanPause { get; init; }

    public bool CanParkUntilNextSchedule { get; init; }

    public bool CanParkUntilFurtherNotice { get; init; }

    public bool CanResumeSchedule { get; init; }

    public bool CanStartForDuration { get; init; }

    public bool CanShowSchedule { get; init; }

    public bool CanEditSchedule { get; init; }
}

