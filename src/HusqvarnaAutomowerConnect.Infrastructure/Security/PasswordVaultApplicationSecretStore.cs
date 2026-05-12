using System.Security.Cryptography;
using System.Text;
using HusqvarnaAutomowerConnect.Core.Errors;
using HusqvarnaAutomowerConnect.Core.Interfaces;

namespace HusqvarnaAutomowerConnect.Infrastructure.Security;

public sealed class PasswordVaultApplicationSecretStore : IApplicationSecretStore
{
    private readonly string filePath;

    public PasswordVaultApplicationSecretStore(string? storageFilePath = null)
    {
        filePath = storageFilePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HusqvarnaAutomowerConnect",
            "application-secret.dat");
    }

    public Task<OperationResult<string?>> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(filePath))
        {
            return Task.FromResult(OperationResult<string?>.Success(null));
        }

        try
        {
            byte[] protectedBytes = File.ReadAllBytes(filePath);
            byte[] plainBytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
            string secret = Encoding.UTF8.GetString(plainBytes);
            return Task.FromResult(OperationResult<string?>.Success(secret));
        }
        catch (Exception exception)
        {
            return Task.FromResult(OperationResult<string?>.Failure(new ApplicationError(
                ApplicationErrorCode.SecureStorageUnavailable,
                "Impossible de charger le secret d'application local.",
                exception.Message)));
        }
    }

    public Task<OperationResult> SaveAsync(string applicationSecret, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(applicationSecret))
        {
            return Task.FromResult(OperationResult.Failure(new ApplicationError(
                ApplicationErrorCode.Validation,
                "Le secret d'application ne peut pas être vide.")));
        }

        try
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(applicationSecret);
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
                "Impossible de supprimer le secret d'application local.",
                exception.Message)));
        }
    }
}
