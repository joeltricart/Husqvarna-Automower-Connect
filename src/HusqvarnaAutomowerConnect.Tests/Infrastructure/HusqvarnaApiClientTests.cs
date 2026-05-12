using System.Net;
using System.Net.Http.Headers;
using System.Text;
using FluentAssertions;
using HusqvarnaAutomowerConnect.Core.Errors;
using HusqvarnaAutomowerConnect.Core.Interfaces;
using HusqvarnaAutomowerConnect.Core.Models;
using HusqvarnaAutomowerConnect.Infrastructure.Api;
using HusqvarnaAutomowerConnect.Infrastructure.Configuration;
using HusqvarnaAutomowerConnect.Infrastructure.Security;

namespace HusqvarnaAutomowerConnect.Tests.Infrastructure;

public sealed class HusqvarnaApiClientTests
{
    [Fact]
    public async Task GetMowersAsync_maps_a_mower_and_adds_official_headers()
    {
        var handler = new QueueHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {
                      "data": [
                        {
                          "type": "mower",
                          "id": "11111111-1111-1111-1111-111111111111",
                          "attributes": {
                            "system": { "name": "Pelouse", "model": "450X", "serialNumber": 701009001 },
                            "battery": { "batteryPercent": 77, "remainingChargingTime": 1234 },
                            "capabilities": { "canConfirmError": true, "headlights": true, "position": true, "stayOutZones": true, "workAreas": true },
                            "mower": { "mode": "MAIN_AREA", "activity": "MOWING", "state": "IN_OPERATION", "inactiveReason": "PLANNING", "errorCode": 0, "isErrorConfirmable": false },
                            "metadata": { "connected": true, "statusTimestamp": 1723449269000 },
                            "positions": [ { "latitude": 57.79051, "longitude": 14.28367 } ]
                          }
                        }
                      ]
                    }
                    """, Encoding.UTF8, OfficialApiContract.JsonApiContentType)
            });

        HusqvarnaApiClient client = CreateClient(handler, new AuthSession
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token"
        });

        OperationResult<IReadOnlyList<Mower>> result = await client.GetMowersAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();

        Mower mower = result.Value!.Single();
        mower.Id.Should().Be("11111111-1111-1111-1111-111111111111");
        mower.Name.Should().Be("Pelouse");
        mower.Model.Should().Be("450X");
        mower.SerialNumber.Should().Be("701009001");
        mower.Battery!.LevelPercent.Should().Be(77);
        mower.Status!.State.Should().Be(MowerState.InOperation);
        mower.Status.Activity.Should().Be(MowerActivity.Mowing);
        mower.Status.Mode.Should().Be(MowerMode.MainArea);
        mower.Location!.Latitude.Should().Be(57.79051);
        mower.Location.Longitude.Should().Be(14.28367);
        mower.LastUpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1723449269000));

        handler.Requests.Should().ContainSingle();
        CapturedRequest request = handler.Requests.Single();
        request.Method.Should().Be(HttpMethod.Get);
        request.RequestUri!.AbsolutePath.Should().EndWith("/mowers");
        request.Headers.Should().ContainKey(OfficialApiContract.ApplicationKeyHeader).WhoseValue.Should().ContainSingle().Which.Should().Be("app-key");
        request.Headers.Should().ContainKey(OfficialApiContract.AuthorizationHeader).WhoseValue.Should().ContainSingle().Which.Should().Be("Bearer access-token");
        request.Headers.Should().ContainKey(OfficialApiContract.AuthorizationProviderHeader).WhoseValue.Should().ContainSingle().Which.Should().Be(OfficialApiContract.AuthorizationProviderValue);
    }

    [Fact]
    public async Task GetMowersAsync_returns_an_empty_list_when_api_returns_no_data()
    {
        var handler = new QueueHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"data":[]}""", Encoding.UTF8, OfficialApiContract.JsonApiContentType)
            });

        HusqvarnaApiClient client = CreateClient(handler, new AuthSession
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token"
        });

        OperationResult<IReadOnlyList<Mower>> result = await client.GetMowersAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Theory]
    [InlineData("401", ApplicationErrorCode.Unauthorized)]
    [InlineData("403", ApplicationErrorCode.Forbidden)]
    [InlineData("429", ApplicationErrorCode.RateLimited)]
    [InlineData("500", ApplicationErrorCode.ServiceUnavailable)]
    [InlineData("503", ApplicationErrorCode.ServiceUnavailable)]
    public async Task GetMowerAsync_maps_http_errors_to_application_errors(
        string statusCode,
        ApplicationErrorCode expectedCode)
    {
        HttpResponseMessage response = new((HttpStatusCode)int.Parse(statusCode))
        {
            Content = new StringContent("""{"errors":[{"detail":"Erreur de test"}]}""", Encoding.UTF8, OfficialApiContract.JsonApiContentType)
        };

        QueueHttpMessageHandler handler = statusCode == "401"
            ? new QueueHttpMessageHandler(response, new HttpResponseMessage((HttpStatusCode)int.Parse(statusCode))
            {
                Content = new StringContent("""{"errors":[{"detail":"Erreur de test"}]}""", Encoding.UTF8, OfficialApiContract.JsonApiContentType)
            })
            : new QueueHttpMessageHandler(response);

        HusqvarnaApiClient client = CreateClient(handler, new AuthSession
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token"
        });

        OperationResult<Mower> result = await client.GetMowerAsync("mower-id", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(expectedCode);
        result.Error.TechnicalMessage.Should().Be("Erreur de test");
    }

    [Fact]
    public async Task GetMowerAsync_returns_not_found_when_payload_has_no_data()
    {
        var handler = new QueueHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"data":null}""", Encoding.UTF8, OfficialApiContract.JsonApiContentType)
            });

        HusqvarnaApiClient client = CreateClient(handler, new AuthSession
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token"
        });

        OperationResult<Mower> result = await client.GetMowerAsync("mower-id", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(ApplicationErrorCode.NotFound);
    }

    [Fact]
    public async Task GetMowerAsync_maps_unknown_values_to_unknown()
    {
        var handler = new QueueHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {
                      "data": {
                        "type": "mower",
                        "id": "mower-1",
                        "attributes": {
                          "system": { "name": "", "model": "EPOS", "serialNumber": 7 },
                          "battery": { "batteryPercent": 101, "remainingChargingTime": 0 },
                          "capabilities": { "canConfirmError": false, "headlights": false, "position": false, "stayOutZones": false, "workAreas": false },
                          "mower": { "mode": "SOMETHING_ELSE", "activity": "RUNNING", "state": "GLITCH", "inactiveReason": "NONE", "errorCode": 0, "isErrorConfirmable": false },
                          "metadata": { "connected": false, "statusTimestamp": 1 }
                        }
                      }
                    }
                    """, Encoding.UTF8, OfficialApiContract.JsonApiContentType)
            });

        HusqvarnaApiClient client = CreateClient(handler, new AuthSession
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token"
        });

        OperationResult<Mower> result = await client.GetMowerAsync("mower-1", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status!.State.Should().Be(MowerState.Offline);
        result.Value.Status.Activity.Should().Be(MowerActivity.Unknown);
        result.Value.Status.Mode.Should().Be(MowerMode.Unknown);
        result.Value.Status.Error.Should().BeNull();
        result.Value.Battery!.LevelPercent.Should().BeNull();
        result.Value.Location.Should().BeNull();
    }

    [Fact]
    public async Task SendCommandAsync_builds_the_pause_payload_and_returns_the_accepted_command()
    {
        var handler = new QueueHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent("""{"data":{"type":"control","id":"cmd-123"}}""", Encoding.UTF8, OfficialApiContract.JsonApiContentType)
            });

        HusqvarnaApiClient client = CreateClient(handler, new AuthSession
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token"
        });

        OperationResult<CommandResult> result = await client.SendCommandAsync(
            "mower-1",
            new MowerCommand { Type = MowerCommandType.Pause },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Success.Should().BeTrue();
        result.Value.Command.Type.Should().Be(MowerCommandType.Pause);
        result.Value.TechnicalCode.Should().Be("cmd-123");

        handler.Requests.Should().ContainSingle();
        handler.Requests.Single().Body.Should().Be("""{"data":{"type":"Pause"}}""");
    }

    [Fact]
    public async Task SendCommandAsync_uses_start_in_work_area_when_work_area_id_is_provided()
    {
        var handler = new QueueHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent("""{"data":{"type":"control","id":"cmd-456"}}""", Encoding.UTF8, OfficialApiContract.JsonApiContentType)
            });

        HusqvarnaApiClient client = CreateClient(handler, new AuthSession
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token"
        });

        OperationResult<CommandResult> result = await client.SendCommandAsync(
            "mower-1",
            new MowerCommand { Type = MowerCommandType.StartForDuration, Duration = TimeSpan.FromMinutes(30), WorkAreaId = "123456" },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        handler.Requests.Single().Body.Should().Contain("""StartInWorkArea""");
        handler.Requests.Single().Body.Should().Contain("workAreaId\":123456");
    }

    [Fact]
    public async Task SendCommandAsync_refreshes_the_token_once_after_a_401()
    {
        var handler = new QueueHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.Unauthorized),
            new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent("""{"data":{"type":"control","id":"cmd-789"}}""", Encoding.UTF8, OfficialApiContract.JsonApiContentType)
            });

        StubAuthClient authClient = new(
            refreshedSession: new AuthSession
            {
                AccessToken = "new-access-token",
                RefreshToken = "new-refresh-token"
            });

        StubSecureTokenStore secureTokenStore = new(new AuthSession
        {
            AccessToken = "stale-access-token",
            RefreshToken = "refresh-token"
        });

        HusqvarnaApiClient client = CreateClient(handler, secureTokenStore, authClient);

        OperationResult<CommandResult> result = await client.SendCommandAsync(
            "mower-1",
            new MowerCommand { Type = MowerCommandType.ResumeSchedule },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        authClient.RefreshCallCount.Should().Be(1);
        handler.Requests.Should().HaveCount(2);
        handler.Requests[0].Headers[OfficialApiContract.AuthorizationHeader].Single().Should().Be("Bearer stale-access-token");
        handler.Requests[1].Headers[OfficialApiContract.AuthorizationHeader].Single().Should().Be("Bearer new-access-token");
    }

    private static HusqvarnaApiClient CreateClient(
        QueueHttpMessageHandler handler,
        AuthSession session,
        StubAuthClient? authClient = null)
    {
        return CreateClient(handler, new StubSecureTokenStore(session), authClient ?? new StubAuthClient(session));
    }

    private static HusqvarnaApiClient CreateClient(
        QueueHttpMessageHandler handler,
        StubSecureTokenStore secureTokenStore,
        StubAuthClient authClient)
    {
        return new HusqvarnaApiClient(
            new HttpClient(handler)
            {
                BaseAddress = new Uri(OfficialApiContract.AutomowerBaseUrl)
            },
            new StubAppSettingsStore(new AppSettings
            {
                ApplicationKey = "app-key",
                RedirectUri = "http://localhost",
                RefreshIntervalSeconds = 60
            }),
            secureTokenStore,
            authClient);
    }

    private sealed class StubAppSettingsStore(AppSettings settings) : IAppSettingsStore
    {
        public Task<OperationResult<AppSettings>> LoadAsync(CancellationToken cancellationToken) =>
            Task.FromResult(OperationResult<AppSettings>.Success(settings));

        public Task<OperationResult> SaveAsync(AppSettings settings, CancellationToken cancellationToken) =>
            Task.FromResult(OperationResult.Success());
    }

    private sealed class StubSecureTokenStore(AuthSession session) : ISecureTokenStore
    {
        public Task<OperationResult> SaveAsync(AuthSession session, CancellationToken cancellationToken) =>
            Task.FromResult(OperationResult.Success());

        public Task<OperationResult<AuthSession?>> LoadAsync(CancellationToken cancellationToken) =>
            Task.FromResult(OperationResult<AuthSession?>.Success(session));

        public Task<OperationResult> DeleteAsync(CancellationToken cancellationToken) =>
            Task.FromResult(OperationResult.Success());
    }

    private sealed class StubAuthClient(AuthSession refreshedSession) : IHusqvarnaAuthClient
    {
        public int RefreshCallCount { get; private set; }

        public Uri BuildAuthorizationUri(AppSettings settings, string state, string? codeChallenge) =>
            new("https://example.invalid/");

        public Task<OperationResult<AuthSession>> ExchangeAuthorizationCodeAsync(
            string code,
            AppSettings settings,
            string clientSecret,
            CancellationToken cancellationToken) =>
            Task.FromResult(OperationResult<AuthSession>.Failure(new ApplicationError(
                ApplicationErrorCode.Unknown,
                "Non utilisé dans ce test.")));

        public Task<OperationResult<AuthSession>> RefreshAccessTokenAsync(
            AuthSession session,
            AppSettings settings,
            CancellationToken cancellationToken)
        {
            RefreshCallCount++;
            return Task.FromResult(OperationResult<AuthSession>.Success(refreshedSession));
        }

        public Task<OperationResult> RevokeAsync(string accessToken, CancellationToken cancellationToken) =>
            Task.FromResult(OperationResult.Success());
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
