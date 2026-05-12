namespace HusqvarnaAutomowerConnect.Core.Models;

public sealed record CommandResult
{
    public bool Success { get; init; }

    public required MowerCommand Command { get; init; }

    public required string Message { get; init; }

    public string? TechnicalCode { get; init; }

    public DateTimeOffset? AcceptedAt { get; init; }

    public bool ShouldRefresh { get; init; }
}

