using System.Net;
using System.Text;
using System.Text.Json;
using HusqvarnaAutomowerConnect.Core.Errors;
using HusqvarnaAutomowerConnect.Core.Interfaces;
using HusqvarnaAutomowerConnect.Core.Models;

namespace HusqvarnaAutomowerConnect.Infrastructure.Api;

public sealed class HusqvarnaApiClient : IHusqvarnaApiClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient httpClient;
    private readonly IAppSettingsStore appSettingsStore;
    private readonly ISecureTokenStore secureTokenStore;
    private readonly IHusqvarnaAuthClient authClient;

    public HusqvarnaApiClient(
        HttpClient httpClient,
        IAppSettingsStore appSettingsStore,
        ISecureTokenStore secureTokenStore,
        IHusqvarnaAuthClient authClient)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this.appSettingsStore = appSettingsStore ?? throw new ArgumentNullException(nameof(appSettingsStore));
        this.secureTokenStore = secureTokenStore ?? throw new ArgumentNullException(nameof(secureTokenStore));
        this.authClient = authClient ?? throw new ArgumentNullException(nameof(authClient));

        if (this.httpClient.BaseAddress is null)
        {
            this.httpClient.BaseAddress = new Uri(OfficialApiContract.AutomowerBaseUrl);
        }
    }

    public async Task<OperationResult<IReadOnlyList<Mower>>> GetMowersAsync(CancellationToken cancellationToken) =>
        await SendAsync<IReadOnlyList<Mower>>(
            _ => new HttpRequestMessage(HttpMethod.Get, "mowers"),
            ParseMowersAsync,
            cancellationToken);

    public async Task<OperationResult<Mower>> GetMowerAsync(string mowerId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(mowerId))
        {
            return OperationResult<Mower>.Failure(new ApplicationError(
                ApplicationErrorCode.Validation,
                "L'identifiant du robot est requis."));
        }

        return await SendAsync<Mower>(
            _ => new HttpRequestMessage(HttpMethod.Get, $"mowers/{Uri.EscapeDataString(mowerId)}"),
            ParseMowerAsync,
            cancellationToken);
    }

    public async Task<OperationResult<CommandResult>> SendCommandAsync(
        string mowerId,
        MowerCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(mowerId))
        {
            return OperationResult<CommandResult>.Failure(new ApplicationError(
                ApplicationErrorCode.Validation,
                "L'identifiant du robot est requis."));
        }

        OperationResult validation = ValidateCommand(command);
        if (!validation.IsSuccess)
        {
            return OperationResult<CommandResult>.Failure(validation.Error!);
        }

        return await SendAsync<CommandResult>(
            _ =>
            {
                HttpRequestMessage request = new(HttpMethod.Post, $"mowers/{Uri.EscapeDataString(mowerId)}/actions");
                request.Content = new StringContent(BuildCommandPayload(command), Encoding.UTF8, OfficialApiContract.JsonApiContentType);
                return request;
            },
            (response, token) => ParseCommandAcceptedAsync(response, command, token),
            cancellationToken);
    }

    private async Task<OperationResult<T>> SendAsync<T>(
        Func<string, HttpRequestMessage> requestFactory,
        Func<HttpResponseMessage, CancellationToken, Task<OperationResult<T>>> successParser,
        CancellationToken cancellationToken)
    {
        OperationResult<AuthenticatedContext> contextResult = await GetAuthenticatedContextAsync(cancellationToken);
        if (!contextResult.IsSuccess)
        {
            return OperationResult<T>.Failure(contextResult.Error!);
        }

        return await SendOnceAsync(contextResult.Value!, requestFactory, successParser, retryOnUnauthorized: true, cancellationToken);
    }

    private async Task<OperationResult<T>> SendOnceAsync<T>(
        AuthenticatedContext context,
        Func<string, HttpRequestMessage> requestFactory,
        Func<HttpResponseMessage, CancellationToken, Task<OperationResult<T>>> successParser,
        bool retryOnUnauthorized,
        CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = requestFactory(context.AccessToken);
        ApplyHeaders(request, context);

        using HttpResponseMessage response = await httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await successParser(response, cancellationToken);
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized && retryOnUnauthorized && context.Session.HasRefreshToken)
        {
            OperationResult<AuthenticatedContext> refreshedResult = await RefreshContextAsync(context, cancellationToken);
            if (refreshedResult.IsSuccess)
            {
                return await SendOnceAsync(
                    refreshedResult.Value!,
                    requestFactory,
                    successParser,
                    retryOnUnauthorized: false,
                    cancellationToken);
            }

            return OperationResult<T>.Failure(refreshedResult.Error!);
        }

        string? technicalMessage = await TryReadTechnicalMessageAsync(response, cancellationToken);
        return OperationResult<T>.Failure(MapApiError(response.StatusCode, technicalMessage));
    }

    private async Task<OperationResult<AuthenticatedContext>> GetAuthenticatedContextAsync(CancellationToken cancellationToken)
    {
        OperationResult<AppSettings> settingsResult = await appSettingsStore.LoadAsync(cancellationToken);
        if (!settingsResult.IsSuccess)
        {
            return OperationResult<AuthenticatedContext>.Failure(settingsResult.Error!);
        }

        IReadOnlyList<string> validationErrors = settingsResult.Value!.Validate();
        if (validationErrors.Count > 0)
        {
            return OperationResult<AuthenticatedContext>.Failure(new ApplicationError(
                ApplicationErrorCode.InvalidConfiguration,
                validationErrors[0]));
        }

        OperationResult<AuthSession?> sessionResult = await secureTokenStore.LoadAsync(cancellationToken);
        if (!sessionResult.IsSuccess)
        {
            return OperationResult<AuthenticatedContext>.Failure(sessionResult.Error!);
        }

        AuthSession? session = sessionResult.Value;
        if (session is null || !session.HasRefreshToken)
        {
            return OperationResult<AuthenticatedContext>.Failure(new ApplicationError(
                ApplicationErrorCode.Unauthorized,
                "Aucune session Husqvarna active n'est disponible."));
        }

        if (session.HasAccessToken)
        {
            return OperationResult<AuthenticatedContext>.Success(new AuthenticatedContext(settingsResult.Value!, session, session.AccessToken!));
        }

        OperationResult<AuthSession> refreshedResult = await authClient.RefreshAccessTokenAsync(session, settingsResult.Value!, cancellationToken);
        if (!refreshedResult.IsSuccess)
        {
            return OperationResult<AuthenticatedContext>.Failure(refreshedResult.Error!);
        }

        AuthSession refreshedSession = refreshedResult.Value!;
        if (!refreshedSession.HasAccessToken)
        {
            return OperationResult<AuthenticatedContext>.Failure(new ApplicationError(
                ApplicationErrorCode.Unauthorized,
                "La session Husqvarna n'a pas fourni de jeton d'accès."));
        }

        OperationResult saveResult = await secureTokenStore.SaveAsync(refreshedSession, cancellationToken);
        if (!saveResult.IsSuccess)
        {
            return OperationResult<AuthenticatedContext>.Failure(saveResult.Error!);
        }

        return OperationResult<AuthenticatedContext>.Success(new AuthenticatedContext(
            settingsResult.Value!,
            refreshedSession,
            refreshedSession.AccessToken!));
    }

    private async Task<OperationResult<AuthenticatedContext>> RefreshContextAsync(
        AuthenticatedContext context,
        CancellationToken cancellationToken)
    {
        OperationResult<AuthSession> refreshedResult = await authClient.RefreshAccessTokenAsync(
            context.Session,
            context.Settings,
            cancellationToken);

        if (!refreshedResult.IsSuccess)
        {
            return OperationResult<AuthenticatedContext>.Failure(refreshedResult.Error!);
        }

        AuthSession refreshedSession = refreshedResult.Value!;
        if (!refreshedSession.HasAccessToken)
        {
            return OperationResult<AuthenticatedContext>.Failure(new ApplicationError(
                ApplicationErrorCode.Unauthorized,
                "La session Husqvarna n'a pas fourni de jeton d'accès."));
        }

        OperationResult saveResult = await secureTokenStore.SaveAsync(refreshedSession, cancellationToken);
        if (!saveResult.IsSuccess)
        {
            return OperationResult<AuthenticatedContext>.Failure(saveResult.Error!);
        }

        return OperationResult<AuthenticatedContext>.Success(new AuthenticatedContext(
            context.Settings,
            refreshedSession,
            refreshedSession.AccessToken!));
    }

    private static void ApplyHeaders(HttpRequestMessage request, AuthenticatedContext context)
    {
        request.Headers.TryAddWithoutValidation(OfficialApiContract.ApplicationKeyHeader, context.Settings.ApplicationKey);
        request.Headers.TryAddWithoutValidation(OfficialApiContract.AuthorizationHeader, $"Bearer {context.AccessToken}");
        request.Headers.TryAddWithoutValidation(OfficialApiContract.AuthorizationProviderHeader, OfficialApiContract.AuthorizationProviderValue);
        request.Headers.TryAddWithoutValidation("Accept", OfficialApiContract.JsonApiContentType);
    }

    private static async Task<OperationResult<IReadOnlyList<Mower>>> ParseMowersAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        string payload = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(payload))
        {
            return OperationResult<IReadOnlyList<Mower>>.Success(Array.Empty<Mower>());
        }

        JsonApiListDocument<JsonApiMowerDataDto>? document;
        try
        {
            document = JsonSerializer.Deserialize<JsonApiListDocument<JsonApiMowerDataDto>>(
                payload,
                SerializerOptions);
        }
        catch (JsonException exception)
        {
            return OperationResult<IReadOnlyList<Mower>>.Failure(new ApplicationError(
                ApplicationErrorCode.Unknown,
                "La réponse Husqvarna est invalide.",
                exception.Message));
        }

        if (document?.Data is null)
        {
            return OperationResult<IReadOnlyList<Mower>>.Success(Array.Empty<Mower>());
        }

        IReadOnlyList<Mower> mowers = document.Data
            .Where(item => item.Attributes is not null)
            .Select(item => MowerMapper.ToMower(item.Id, item.Attributes!))
            .Where(item => !string.IsNullOrWhiteSpace(item.Id))
            .ToArray();

        return OperationResult<IReadOnlyList<Mower>>.Success(mowers);
    }

    private static async Task<OperationResult<Mower>> ParseMowerAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        string payload = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(payload))
        {
            return OperationResult<Mower>.Failure(new ApplicationError(
                ApplicationErrorCode.NotFound,
                "Le robot demandé est introuvable."));
        }

        JsonApiDocument<JsonApiMowerDataDto>? document;
        try
        {
            document = JsonSerializer.Deserialize<JsonApiDocument<JsonApiMowerDataDto>>(
                payload,
                SerializerOptions);
        }
        catch (JsonException exception)
        {
            return OperationResult<Mower>.Failure(new ApplicationError(
                ApplicationErrorCode.Unknown,
                "La réponse Husqvarna est invalide.",
                exception.Message));
        }

        if (document?.Data?.Attributes is null || string.IsNullOrWhiteSpace(document.Data.Id))
        {
            return OperationResult<Mower>.Failure(new ApplicationError(
                ApplicationErrorCode.NotFound,
                "Le robot demandé est introuvable."));
        }

        return OperationResult<Mower>.Success(MowerMapper.ToMower(document.Data.Id, document.Data.Attributes));
    }

    private static async Task<OperationResult<CommandResult>> ParseCommandAcceptedAsync(
        HttpResponseMessage response,
        MowerCommand command,
        CancellationToken cancellationToken)
    {
        string payload = await response.Content.ReadAsStringAsync(cancellationToken);
        JsonApiDocument<JsonApiCommandAcceptedDto>? document = null;
        if (!string.IsNullOrWhiteSpace(payload))
        {
            try
            {
                document = JsonSerializer.Deserialize<JsonApiDocument<JsonApiCommandAcceptedDto>>(payload, SerializerOptions);
            }
            catch (JsonException exception)
            {
                return OperationResult<CommandResult>.Failure(new ApplicationError(
                    ApplicationErrorCode.Unknown,
                    "La réponse Husqvarna est invalide.",
                    exception.Message));
            }
        }

        string? technicalCode = document?.Data?.Id;
        string? commandType = document?.Data?.Type;

        return OperationResult<CommandResult>.Success(new CommandResult
        {
            Success = true,
            Command = command,
            Message = "Commande envoyée avec succès.",
            TechnicalCode = technicalCode ?? commandType ?? "commande",
            AcceptedAt = DateTimeOffset.UtcNow,
            ShouldRefresh = true
        });
    }

    private static ApplicationError MapApiError(HttpStatusCode statusCode, string? technicalMessage)
    {
        int code = (int)statusCode;
        return ApiErrorMapper.FromStatusCode(code, technicalMessage);
    }

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

    private static OperationResult ValidateCommand(MowerCommand command)
    {
        if (command.Type == MowerCommandType.StartForDuration &&
            (!command.Duration.HasValue || command.Duration <= TimeSpan.Zero))
        {
            return OperationResult.Failure(new ApplicationError(
                ApplicationErrorCode.Validation,
                "La durée de tonte temporaire doit être strictement positive."));
        }

        if (command.Type == MowerCommandType.StartForDuration &&
            !string.IsNullOrWhiteSpace(command.WorkAreaId) &&
            !long.TryParse(command.WorkAreaId, out _))
        {
            return OperationResult.Failure(new ApplicationError(
                ApplicationErrorCode.Validation,
                "L'identifiant de zone de travail doit être numérique."));
        }

        return OperationResult.Success();
    }

    private static string BuildCommandPayload(MowerCommand command) =>
        command.Type switch
        {
            MowerCommandType.Pause => """
                {"data":{"type":"Pause"}}
                """,
            MowerCommandType.ResumeSchedule => """
                {"data":{"type":"ResumeSchedule"}}
                """,
            MowerCommandType.ParkUntilNextSchedule => """
                {"data":{"type":"ParkUntilNextSchedule"}}
                """,
            MowerCommandType.ParkUntilFurtherNotice => """
                {"data":{"type":"ParkUntilFurtherNotice"}}
                """,
            MowerCommandType.StartForDuration when string.IsNullOrWhiteSpace(command.WorkAreaId) => BuildStartPayload("Start", command),
            MowerCommandType.StartForDuration => BuildStartPayload("StartInWorkArea", command),
            _ => throw new NotSupportedException($"La commande {command.Type} n'est pas gérée.")
        };

    private static string BuildStartPayload(string type, MowerCommand command)
    {
        int durationMinutes = Math.Max(1, (int)Math.Ceiling(command.Duration!.Value.TotalMinutes));
        long? workAreaId = long.TryParse(command.WorkAreaId, out long parsedWorkAreaId) ? parsedWorkAreaId : null;

        object payload = workAreaId.HasValue
            ? new
            {
                data = new
                {
                    type,
                    @attributes = new
                    {
                        duration = durationMinutes,
                        workAreaId = workAreaId.Value
                    }
                }
            }
            : new
            {
                data = new
                {
                    type,
                    @attributes = new
                    {
                        duration = durationMinutes
                    }
                }
            };

        return JsonSerializer.Serialize(payload, SerializerOptions);
    }

    private sealed record AuthenticatedContext(
        AppSettings Settings,
        AuthSession Session,
        string AccessToken);

    internal sealed record JsonApiDocument<T>
    {
        public T? Data { get; init; }
    }

    internal sealed record JsonApiListDocument<T>
    {
        public IReadOnlyList<T>? Data { get; init; }
    }

    internal sealed record JsonApiErrorDocumentDto
    {
        public IReadOnlyList<JsonApiErrorDto>? Errors { get; init; }
    }

    internal sealed record JsonApiErrorDto
    {
        public string? Id { get; init; }

        public string? Status { get; init; }

        public string? Code { get; init; }

        public string? Title { get; init; }

        public string? Detail { get; init; }
    }

    internal sealed record JsonApiMowerDataDto
    {
        public string? Type { get; init; }

        public string? Id { get; init; }

        public JsonApiMowerAttributesDto? Attributes { get; init; }
    }

    internal sealed record JsonApiCommandAcceptedDto
    {
        public string? Type { get; init; }

        public string? Id { get; init; }
    }

    internal sealed record JsonApiMowerAttributesDto
    {
        public JsonApiSystemDto? System { get; init; }

        public JsonApiBatteryDto? Battery { get; init; }

        public JsonApiCapabilitiesDto? Capabilities { get; init; }

        public JsonApiMowerAppDto? Mower { get; init; }

        public JsonApiMetadataDto? Metadata { get; init; }

        public IReadOnlyList<JsonApiPositionDto>? Positions { get; init; }
    }

    internal sealed record JsonApiSystemDto
    {
        public string? Name { get; init; }

        public string? Model { get; init; }

        public long? SerialNumber { get; init; }
    }

    internal sealed record JsonApiBatteryDto
    {
        public int? BatteryPercent { get; init; }

        public long? RemainingChargingTime { get; init; }
    }

    internal sealed record JsonApiCapabilitiesDto
    {
        public bool? CanConfirmError { get; init; }

        public bool? Headlights { get; init; }

        public bool? Position { get; init; }

        public bool? StayOutZones { get; init; }

        public bool? WorkAreas { get; init; }
    }

    internal sealed record JsonApiMowerAppDto
    {
        public string? Mode { get; init; }

        public string? Activity { get; init; }

        public string? InactiveReason { get; init; }

        public string? State { get; init; }

        public long? WorkAreaId { get; init; }

        public long? ErrorCode { get; init; }

        public long? ErrorCodeTimestamp { get; init; }

        public bool? IsErrorConfirmable { get; init; }
    }

    internal sealed record JsonApiMetadataDto
    {
        public bool? Connected { get; init; }

        public long? StatusTimestamp { get; init; }
    }

    internal sealed record JsonApiPositionDto
    {
        public double? Latitude { get; init; }

        public double? Longitude { get; init; }
    }
}
