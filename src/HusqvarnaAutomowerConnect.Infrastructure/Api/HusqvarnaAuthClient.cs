using System.Text.Json;
using System.Text.Json.Serialization;
using HusqvarnaAutomowerConnect.Core.Errors;
using HusqvarnaAutomowerConnect.Core.Interfaces;
using HusqvarnaAutomowerConnect.Core.Models;
using HusqvarnaAutomowerConnect.Infrastructure.Configuration;

namespace HusqvarnaAutomowerConnect.Infrastructure.Api;

public sealed class HusqvarnaAuthClient : IHusqvarnaAuthClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient httpClient;
    private readonly IClock clock;

    public HusqvarnaAuthClient(HttpClient httpClient, IClock clock)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this.clock = clock ?? throw new ArgumentNullException(nameof(clock));

        if (this.httpClient.BaseAddress is null)
        {
            this.httpClient.BaseAddress = new Uri(OfficialApiContract.AuthenticationBaseUrl);
        }
    }

    public Uri BuildAuthorizationUri(AppSettings settings, string state, string? codeChallenge)
    {
        IReadOnlyList<string> validationErrors = settings.Validate();
        if (validationErrors.Count > 0)
        {
            throw new ArgumentException(validationErrors[0], nameof(settings));
        }

        if (string.IsNullOrWhiteSpace(state))
        {
            throw new ArgumentException("L'état OAuth est requis.", nameof(state));
        }

        UriBuilder builder = new(new Uri(new Uri(OfficialApiContract.AuthenticationBaseUrl), OfficialApiContract.Authentication.AuthorizePath));
        List<string> queryParts =
        [
            $"client_id={Uri.EscapeDataString(settings.ApplicationKey)}",
            $"redirect_uri={Uri.EscapeDataString(settings.RedirectUri)}",
            "language=fr",
            $"state={Uri.EscapeDataString(state)}",
            "response_type=code"
        ];

        if (!string.IsNullOrWhiteSpace(codeChallenge))
        {
            queryParts.Add($"code_challenge={Uri.EscapeDataString(codeChallenge)}");
            queryParts.Add("code_challenge_method=S256");
        }

        builder.Query = string.Join('&', queryParts);
        return builder.Uri;
    }

    public Task<OperationResult<AuthSession>> ExchangeAuthorizationCodeAsync(
        string code,
        AppSettings settings,
        string clientSecret,
        CancellationToken cancellationToken)
    {
        return ExchangeAuthorizationCodeInternalAsync(code, settings, clientSecret, cancellationToken);
    }

    public async Task<OperationResult<AuthSession>> RefreshAccessTokenAsync(
        AuthSession session,
        AppSettings settings,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<string> validationErrors = settings.Validate();
        if (validationErrors.Count > 0)
        {
            return OperationResult<AuthSession>.Failure(new ApplicationError(
                ApplicationErrorCode.InvalidConfiguration,
                validationErrors[0]));
        }

        if (!session.HasRefreshToken)
        {
            return OperationResult<AuthSession>.Failure(new ApplicationError(
                ApplicationErrorCode.Validation,
                "Aucun refresh token n'est disponible pour renouveler la session."));
        }

        using HttpRequestMessage request = new(HttpMethod.Post, OfficialApiContract.Authentication.TokenPath)
        {
            Content = new FormUrlEncodedContent(BuildTokenRequestFields(settings, session))
        };

        using HttpResponseMessage response = await httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string? technicalMessage = await TryReadTechnicalMessageAsync(response, cancellationToken);
            return OperationResult<AuthSession>.Failure(ApiErrorMapper.FromStatusCode((int)response.StatusCode, technicalMessage));
        }

        string payload = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(payload))
        {
            return OperationResult<AuthSession>.Failure(new ApplicationError(
                ApplicationErrorCode.Unknown,
                "La réponse OAuth Husqvarna est vide."));
        }

        OAuthTokenResponse? tokenResponse;
        try
        {
            tokenResponse = JsonSerializer.Deserialize<OAuthTokenResponse>(payload, SerializerOptions);
        }
        catch (JsonException exception)
        {
            return OperationResult<AuthSession>.Failure(new ApplicationError(
                ApplicationErrorCode.Unknown,
                "La réponse OAuth Husqvarna est invalide.",
                exception.Message));
        }

        if (string.IsNullOrWhiteSpace(tokenResponse?.AccessToken))
        {
            return OperationResult<AuthSession>.Failure(new ApplicationError(
                ApplicationErrorCode.Unknown,
                "La réponse OAuth Husqvarna n'a pas fourni de jeton d'accès."));
        }

        return OperationResult<AuthSession>.Success(new AuthSession
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = string.IsNullOrWhiteSpace(tokenResponse.RefreshToken) ? session.RefreshToken : tokenResponse.RefreshToken,
            ExpiresAt = tokenResponse.ExpiresIn.HasValue && tokenResponse.ExpiresIn.Value > 0
                ? clock.UtcNow.AddSeconds(tokenResponse.ExpiresIn.Value)
                : null,
            TokenType = string.IsNullOrWhiteSpace(tokenResponse.TokenType) ? "Bearer" : tokenResponse.TokenType,
            Scopes = ParseScopes(tokenResponse.Scope),
            Provider = tokenResponse.Provider,
            UserId = tokenResponse.UserId
        });
    }

    public async Task<OperationResult> RevokeAsync(
        string accessToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return OperationResult.Failure(new ApplicationError(
                ApplicationErrorCode.Validation,
                "Un jeton d'accès est requis pour révoquer la session."));
        }

        using HttpRequestMessage request = new(HttpMethod.Post, OfficialApiContract.Authentication.RevokePath)
        {
            Content = new FormUrlEncodedContent([new KeyValuePair<string, string>("token", accessToken)])
        };
        request.Headers.TryAddWithoutValidation(OfficialApiContract.AuthorizationHeader, $"Bearer {accessToken}");

        using HttpResponseMessage response = await httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return OperationResult.Success();
        }

        string? technicalMessage = await TryReadTechnicalMessageAsync(response, cancellationToken);
        return OperationResult.Failure(ApiErrorMapper.FromStatusCode((int)response.StatusCode, technicalMessage));
    }

    private async Task<OperationResult<AuthSession>> ExchangeAuthorizationCodeInternalAsync(
        string code,
        AppSettings settings,
        string clientSecret,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<string> validationErrors = settings.Validate();
        if (validationErrors.Count > 0)
        {
            return OperationResult<AuthSession>.Failure(new ApplicationError(
                ApplicationErrorCode.InvalidConfiguration,
                validationErrors[0]));
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            return OperationResult<AuthSession>.Failure(new ApplicationError(
                ApplicationErrorCode.Validation,
                "Le code d'autorisation est requis."));
        }

        if (string.IsNullOrWhiteSpace(clientSecret))
        {
            return OperationResult<AuthSession>.Failure(new ApplicationError(
                ApplicationErrorCode.Validation,
                "Le secret d'application est requis pour l'échange du code."));
        }

        using HttpRequestMessage request = new(HttpMethod.Post, OfficialApiContract.Authentication.TokenPath)
        {
            Content = new FormUrlEncodedContent(BuildAuthorizationCodeFields(settings, code, clientSecret))
        };

        using HttpResponseMessage response = await httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string? technicalMessage = await TryReadTechnicalMessageAsync(response, cancellationToken);
            return OperationResult<AuthSession>.Failure(ApiErrorMapper.FromStatusCode((int)response.StatusCode, technicalMessage));
        }

        string payload = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(payload))
        {
            return OperationResult<AuthSession>.Failure(new ApplicationError(
                ApplicationErrorCode.Unknown,
                "La réponse OAuth Husqvarna est vide."));
        }

        OAuthTokenResponse? tokenResponse;
        try
        {
            tokenResponse = JsonSerializer.Deserialize<OAuthTokenResponse>(payload, SerializerOptions);
        }
        catch (JsonException exception)
        {
            return OperationResult<AuthSession>.Failure(new ApplicationError(
                ApplicationErrorCode.Unknown,
                "La réponse OAuth Husqvarna est invalide.",
                exception.Message));
        }

        if (string.IsNullOrWhiteSpace(tokenResponse?.AccessToken))
        {
            return OperationResult<AuthSession>.Failure(new ApplicationError(
                ApplicationErrorCode.Unknown,
                "La réponse OAuth Husqvarna n'a pas fourni de jeton d'accès."));
        }

        return OperationResult<AuthSession>.Success(new AuthSession
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            ExpiresAt = tokenResponse.ExpiresIn.HasValue && tokenResponse.ExpiresIn.Value > 0
                ? clock.UtcNow.AddSeconds(tokenResponse.ExpiresIn.Value)
                : null,
            TokenType = string.IsNullOrWhiteSpace(tokenResponse.TokenType) ? "Bearer" : tokenResponse.TokenType,
            Scopes = ParseScopes(tokenResponse.Scope),
            Provider = tokenResponse.Provider,
            UserId = tokenResponse.UserId
        });
    }

    private static IReadOnlyList<KeyValuePair<string, string>> BuildTokenRequestFields(AppSettings settings, AuthSession session)
    {
        List<KeyValuePair<string, string>> fields =
        [
            new("grant_type", "refresh_token"),
            new("client_id", settings.ApplicationKey),
            new("refresh_token", session.RefreshToken!)
        ];

        if (session.Scopes.Count > 0)
        {
            fields.Add(new KeyValuePair<string, string>("scope", string.Join(' ', session.Scopes)));
        }

        return fields;
    }

    private static IReadOnlyList<KeyValuePair<string, string>> BuildAuthorizationCodeFields(
        AppSettings settings,
        string code,
        string clientSecret)
    {
        List<KeyValuePair<string, string>> fields =
        [
            new("grant_type", "authorization_code"),
            new("client_id", settings.ApplicationKey),
            new("client_secret", clientSecret),
            new("code", code),
            new("redirect_uri", settings.RedirectUri)
        ];

        return fields;
    }

    private static IReadOnlyList<string> ParseScopes(string? scope) =>
        string.IsNullOrWhiteSpace(scope)
            ? []
            : scope.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static async Task<string?> TryReadTechnicalMessageAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        string payload = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        try
        {
            JsonApiErrorDocumentDto? document = JsonSerializer.Deserialize<JsonApiErrorDocumentDto>(payload, SerializerOptions);
            JsonApiErrorDto? error = document?.Errors?.FirstOrDefault();
            return error?.Detail ?? error?.Title ?? error?.Code;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed record OAuthTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; init; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; init; }

        [JsonPropertyName("expires_in")]
        public int? ExpiresIn { get; init; }

        [JsonPropertyName("scope")]
        public string? Scope { get; init; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; init; }

        [JsonPropertyName("provider")]
        public string? Provider { get; init; }

        [JsonPropertyName("user_id")]
        public string? UserId { get; init; }
    }

    private sealed record JsonApiErrorDocumentDto
    {
        public IReadOnlyList<JsonApiErrorDto>? Errors { get; init; }
    }

    private sealed record JsonApiErrorDto
    {
        public string? Code { get; init; }

        public string? Title { get; init; }

        public string? Detail { get; init; }
    }
}
