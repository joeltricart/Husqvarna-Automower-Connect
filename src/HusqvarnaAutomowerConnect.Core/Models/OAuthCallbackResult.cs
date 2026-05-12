namespace HusqvarnaAutomowerConnect.Core.Models;

public sealed record OAuthCallbackResult
{
    public string? Code { get; init; }

    public string? State { get; init; }

    public string? Error { get; init; }

    public string? ErrorDescription { get; init; }
}
