namespace HusqvarnaAutomowerConnect.Core.Models;

public sealed record AuthSession
{
    public string? AccessToken { get; init; }

    public string? RefreshToken { get; init; }

    public DateTimeOffset? ExpiresAt { get; init; }

    public string TokenType { get; init; } = "Bearer";

    public IReadOnlyList<string> Scopes { get; init; } = [];

    public string? Provider { get; init; }

    public string? UserId { get; init; }

    public bool HasAccessToken => !string.IsNullOrWhiteSpace(AccessToken);

    public bool HasRefreshToken => !string.IsNullOrWhiteSpace(RefreshToken);
}

