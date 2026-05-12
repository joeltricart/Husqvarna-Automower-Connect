using System.Net;
using System.Text;
using FluentAssertions;
using HusqvarnaAutomowerConnect.Core.Errors;
using HusqvarnaAutomowerConnect.Core.Interfaces;
using HusqvarnaAutomowerConnect.Core.Models;
using HusqvarnaAutomowerConnect.Infrastructure.Api;
using HusqvarnaAutomowerConnect.Infrastructure.Configuration;

namespace HusqvarnaAutomowerConnect.Tests.Infrastructure;

public sealed class HusqvarnaAuthClientTests
{
    [Fact]
    public async Task RefreshAccessTokenAsync_uses_the_official_token_endpoint()
    {
        var handler = new QueueHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("""
                    {
                      "access_token": "new-access-token",
                      "refresh_token": "new-refresh-token",
                      "expires_in": 3600,
                      "scope": "mowers",
                      "provider": "husqvarna",
                      "user_id": "user-1",
                      "token_type": "Bearer"
                    }
                    """, Encoding.UTF8, "application/json")
            });

        HusqvarnaAuthClient client = CreateClient(handler, new FakeClock(DateTimeOffset.Parse("2026-05-12T08:00:00Z")));

        OperationResult<AuthSession> result = await client.RefreshAccessTokenAsync(
            new AuthSession
            {
                RefreshToken = "refresh-token"
            },
            new AppSettings
            {
                ApplicationKey = "app-key",
                RedirectUri = "http://localhost",
                RefreshIntervalSeconds = 60
            },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("new-access-token");
        result.Value.RefreshToken.Should().Be("new-refresh-token");
        result.Value.ExpiresAt.Should().Be(DateTimeOffset.Parse("2026-05-12T09:00:00Z"));
        result.Value.Provider.Should().Be("husqvarna");
        result.Value.UserId.Should().Be("user-1");

        handler.Requests.Should().ContainSingle();
        CapturedRequest request = handler.Requests.Single();
        request.Method.Should().Be(HttpMethod.Post);
        request.RequestUri!.AbsolutePath.Should().EndWith("/oauth2/token");
        request.Body.Should().Contain("grant_type=refresh_token");
        request.Body.Should().Contain("client_id=app-key");
        request.Body.Should().Contain("refresh_token=refresh-token");
    }

    [Fact]
    public void BuildAuthorizationUri_uses_the_official_authorize_endpoint()
    {
        HusqvarnaAuthClient client = CreateClient(new QueueHttpMessageHandler(), new FakeClock(DateTimeOffset.UtcNow));

        Uri uri = client.BuildAuthorizationUri(
            new AppSettings
            {
                ApplicationKey = "app-key",
                RedirectUri = "http://localhost",
                RefreshIntervalSeconds = 60
            },
            "state-123",
            null);

        uri.AbsolutePath.Should().EndWith("/oauth2/authorize");
        uri.Query.Should().Contain("client_id=app-key");
        uri.Query.Should().Contain("redirect_uri=http%3A%2F%2Flocalhost");
        uri.Query.Should().Contain("state=state-123");
    }

    [Fact]
    public async Task ExchangeAuthorizationCodeAsync_uses_the_official_token_endpoint()
    {
        var handler = new QueueHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("""
                    {
                      "access_token": "access-token",
                      "refresh_token": "refresh-token",
                      "expires_in": 3600,
                      "scope": "mowers",
                      "provider": "husqvarna",
                      "user_id": "user-1",
                      "token_type": "Bearer"
                    }
                    """, Encoding.UTF8, "application/json")
            });

        HusqvarnaAuthClient client = CreateClient(handler, new FakeClock(DateTimeOffset.Parse("2026-05-12T08:00:00Z")));

        OperationResult<AuthSession> result = await client.ExchangeAuthorizationCodeAsync(
            "auth-code",
            new AppSettings
            {
                ApplicationKey = "app-key",
                RedirectUri = "http://localhost",
                RefreshIntervalSeconds = 60
            },
            "client-secret",
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("access-token");
        result.Value.RefreshToken.Should().Be("refresh-token");

        handler.Requests.Should().ContainSingle();
        CapturedRequest request = handler.Requests.Single();
        request.Method.Should().Be(HttpMethod.Post);
        request.RequestUri!.AbsolutePath.Should().EndWith("/oauth2/token");
        request.Body.Should().Contain("grant_type=authorization_code");
        request.Body.Should().Contain("client_id=app-key");
        request.Body.Should().Contain("client_secret=client-secret");
        request.Body.Should().Contain("code=auth-code");
        request.Body.Should().Contain("redirect_uri=http%3A%2F%2Flocalhost");
    }

    [Fact]
    public async Task RevokeAsync_uses_the_official_revoke_endpoint()
    {
        var handler = new QueueHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.NoContent));

        HusqvarnaAuthClient client = CreateClient(handler, new FakeClock(DateTimeOffset.Parse("2026-05-12T08:00:00Z")));

        OperationResult result = await client.RevokeAsync("access-token", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        handler.Requests.Should().ContainSingle();
        CapturedRequest request = handler.Requests.Single();
        request.Method.Should().Be(HttpMethod.Post);
        request.RequestUri!.AbsolutePath.Should().EndWith("/oauth2/revoke");
        request.Headers.Should().ContainKey("Authorization").WhoseValue.Should().ContainSingle().Which.Should().Be("Bearer access-token");
        request.Body.Should().Contain("token=access-token");
    }

    private static HusqvarnaAuthClient CreateClient(QueueHttpMessageHandler handler, FakeClock clock) =>
        new(new HttpClient(handler)
        {
            BaseAddress = new Uri(OfficialApiContract.AuthenticationBaseUrl)
        }, clock);

    private sealed class FakeClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }

    private sealed class QueueHttpMessageHandler(params HttpResponseMessage[] responses) : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> responses = new(responses);

        public List<CapturedRequest> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string? body = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            Dictionary<string, IReadOnlyList<string>> headers = request.Headers
                .ToDictionary(
                    header => header.Key,
                    header => (IReadOnlyList<string>)header.Value.ToArray());

            Requests.Add(new CapturedRequest(
                request.Method,
                request.RequestUri,
                headers,
                body));

            if (responses.Count == 0)
            {
                throw new InvalidOperationException("Aucune réponse de test n'est disponible.");
            }

            return responses.Dequeue();
        }
    }

    private sealed record CapturedRequest(
        HttpMethod Method,
        Uri? RequestUri,
        IReadOnlyDictionary<string, IReadOnlyList<string>> Headers,
        string? Body);
}
