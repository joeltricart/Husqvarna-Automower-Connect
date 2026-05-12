using System.Text.Json;
using HusqvarnaAutomowerConnect.Core.Errors;
using HusqvarnaAutomowerConnect.Core.Interfaces;
using HusqvarnaAutomowerConnect.Core.Models;

namespace HusqvarnaAutomowerConnect.Infrastructure.Configuration;

public sealed class LocalAppSettingsStore(string? settingsFilePath = null) : IAppSettingsStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string filePath = settingsFilePath ?? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "HusqvarnaAutomowerConnect",
        "appsettings.Local.json");

    public async Task<OperationResult<AppSettings>> LoadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            return OperationResult<AppSettings>.Success(new AppSettings());
        }

        try
        {
            await using FileStream stream = File.OpenRead(filePath);
            AppSettingsFile? settingsFile = await JsonSerializer.DeserializeAsync<AppSettingsFile>(
                stream,
                SerializerOptions,
                cancellationToken);

            AppSettings settings = new()
            {
                ApplicationKey = settingsFile?.Husqvarna.ApplicationKey ?? string.Empty,
                RedirectUri = settingsFile?.Husqvarna.RedirectUri ?? "http://localhost",
                RefreshIntervalSeconds = settingsFile?.Husqvarna.RefreshIntervalSeconds ?? AppSettings.DefaultRefreshIntervalSeconds,
                MinimumLogLevel = settingsFile?.Logging.MinimumLevel ?? "Information"
            };

            return OperationResult<AppSettings>.Success(settings);
        }
        catch (JsonException exception)
        {
            return OperationResult<AppSettings>.Failure(new ApplicationError(
                ApplicationErrorCode.InvalidConfiguration,
                "Le fichier de configuration local est invalide.",
                exception.Message));
        }
        catch (IOException exception)
        {
            return OperationResult<AppSettings>.Failure(new ApplicationError(
                ApplicationErrorCode.InvalidConfiguration,
                "Impossible de lire la configuration locale.",
                exception.Message));
        }
    }

    public async Task<OperationResult> SaveAsync(AppSettings settings, CancellationToken cancellationToken)
    {
        IReadOnlyList<string> validationErrors = settings.Validate();
        if (validationErrors.Count > 0)
        {
            return OperationResult.Failure(new ApplicationError(
                ApplicationErrorCode.InvalidConfiguration,
                validationErrors[0]));
        }

        try
        {
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            AppSettingsFile settingsFile = new()
            {
                Husqvarna = new HusqvarnaSettingsSection
                {
                    ApplicationKey = settings.ApplicationKey,
                    RedirectUri = settings.RedirectUri,
                    RefreshIntervalSeconds = settings.RefreshIntervalSeconds
                },
                Logging = new LoggingSettingsSection
                {
                    MinimumLevel = settings.MinimumLogLevel
                }
            };

            await using FileStream stream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(stream, settingsFile, SerializerOptions, cancellationToken);
            return OperationResult.Success();
        }
        catch (IOException exception)
        {
            return OperationResult.Failure(new ApplicationError(
                ApplicationErrorCode.InvalidConfiguration,
                "Impossible d'enregistrer la configuration locale.",
                exception.Message));
        }
    }

    private sealed record AppSettingsFile
    {
        public HusqvarnaSettingsSection Husqvarna { get; init; } = new();

        public LoggingSettingsSection Logging { get; init; } = new();
    }

    private sealed record HusqvarnaSettingsSection
    {
        public string ApplicationKey { get; init; } = string.Empty;

        public string RedirectUri { get; init; } = "http://localhost";

        public int RefreshIntervalSeconds { get; init; } = AppSettings.DefaultRefreshIntervalSeconds;
    }

    private sealed record LoggingSettingsSection
    {
        public string MinimumLevel { get; init; } = "Information";
    }
}

