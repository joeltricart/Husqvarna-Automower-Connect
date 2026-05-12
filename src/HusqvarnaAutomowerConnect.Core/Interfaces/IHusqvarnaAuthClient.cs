using HusqvarnaAutomowerConnect.Core.Errors;
using HusqvarnaAutomowerConnect.Core.Models;

namespace HusqvarnaAutomowerConnect.Core.Interfaces;

public interface IHusqvarnaAuthClient
{
    Uri BuildAuthorizationUri(AppSettings settings, string state, string? codeChallenge);

    Task<OperationResult<AuthSession>> ExchangeAuthorizationCodeAsync(
        string code,
        AppSettings settings,
        string clientSecret,
        CancellationToken cancellationToken);

    Task<OperationResult<AuthSession>> RefreshAccessTokenAsync(
        AuthSession session,
        AppSettings settings,
        CancellationToken cancellationToken);

    Task<OperationResult> RevokeAsync(
        string accessToken,
        CancellationToken cancellationToken);
}
