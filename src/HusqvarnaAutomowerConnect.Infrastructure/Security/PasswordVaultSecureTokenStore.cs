using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HusqvarnaAutomowerConnect.Core.Errors;
using HusqvarnaAutomowerConnect.Core.Interfaces;
using HusqvarnaAutomowerConnect.Core.Models;

namespace HusqvarnaAutomowerConnect.Infrastructure.Security;

public sealed class PasswordVaultSecureTokenStore : ISecureTokenStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    private readonly string filePath;

    public PasswordVaultSecureTokenStore(string? storageFilePath = null)
    {
        filePath = storageFilePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HusqvarnaAutomowerConnect",
            "secure-session.dat");
    }

    public Task<OperationResult> SaveAsync(AuthSession session, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!session.HasRefreshToken)
        {
            return Task.FromResult(OperationResult.Failure(new ApplicationError(
                ApplicationErrorCode.Validation,
                "Aucun refresh token n'est disponible pour la sauvegarde sécurisée.")));
        }

        try
        {
            SecureAuthSession storedSession = new()
            {
                AccessToken = session.AccessToken,
                RefreshToken = session.RefreshToken!,
                ExpiresAt = session.ExpiresAt,
                TokenType = session.TokenType,
                Scopes = session.Scopes.ToArray(),
                Provider = session.Provider,
                UserId = session.UserId
            };

            byte[] plainBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(storedSession, SerializerOptions));
            byte[] protectedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);

            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(filePath, protectedBytes);
            return Task.FromResult(OperationResult.Success());
        }
        catch (Exception exception)
        {
            return Task.FromResult(OperationResult.Failure(new ApplicationError(
                ApplicationErrorCode.SecureStorageUnavailable,
                "Le stockage sécurisé Windows est indisponible.",
                exception.Message)));
        }
    }

    public Task<OperationResult<AuthSession?>> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(filePath))
        {
            return Task.FromResult(OperationResult<AuthSession?>.Success(null));
        }

        try
        {
            byte[] protectedBytes = File.ReadAllBytes(filePath);
            byte[] plainBytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
            string payload = Encoding.UTF8.GetString(plainBytes);
            SecureAuthSession? storedSession = JsonSerializer.Deserialize<SecureAuthSession>(payload, SerializerOptions);

            AuthSession session = new()
            {
                AccessToken = storedSession?.AccessToken,
                RefreshToken = storedSession?.RefreshToken,
                ExpiresAt = storedSession?.ExpiresAt,
                TokenType = storedSession?.TokenType ?? "Bearer",
                Scopes = storedSession?.Scopes ?? [],
                Provider = storedSession?.Provider,
                UserId = storedSession?.UserId
            };

            return Task.FromResult(OperationResult<AuthSession?>.Success(session));
        }
        catch (Exception exception)
        {
            return Task.FromResult(OperationResult<AuthSession?>.Failure(new ApplicationError(
                ApplicationErrorCode.SecureStorageUnavailable,
                "Impossible de charger la session stockée localement.",
                exception.Message)));
        }
    }

    public Task<OperationResult> DeleteAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.FromResult(OperationResult.Success());
        }
        catch (Exception exception)
        {
            return Task.FromResult(OperationResult.Failure(new ApplicationError(
                ApplicationErrorCode.SecureStorageUnavailable,
                "Impossible de supprimer la session locale.",
                exception.Message)));
        }
    }

    private sealed record SecureAuthSession
    {
        public string? AccessToken { get; init; }

        public required string RefreshToken { get; init; }

        public DateTimeOffset? ExpiresAt { get; init; }

        public string TokenType { get; init; } = "Bearer";

        public IReadOnlyList<string> Scopes { get; init; } = [];

        public string? Provider { get; init; }

        public string? UserId { get; init; }
    }
}
