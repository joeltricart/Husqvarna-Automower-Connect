namespace HusqvarnaAutomowerConnect.Core.Models;

public enum MowerErrorSeverity
{
    Unknown = 0,
    Info = 1,
    Warning = 2,
    Critical = 3
}

public sealed record MowerError
{
    public string? Code { get; init; }

    public string? Message { get; init; }

    public MowerErrorSeverity Severity { get; init; } = MowerErrorSeverity.Unknown;

    public DateTimeOffset? OccurredAt { get; init; }

    public bool? IsConfirmable { get; init; }
}

