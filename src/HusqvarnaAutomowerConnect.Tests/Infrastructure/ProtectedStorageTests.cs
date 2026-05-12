using FluentAssertions;
using HusqvarnaAutomowerConnect.Core.Errors;
using HusqvarnaAutomowerConnect.Core.Models;
using HusqvarnaAutomowerConnect.Infrastructure.Security;

namespace HusqvarnaAutomowerConnect.Tests.Infrastructure;

public sealed class ProtectedStorageTests
{
    [Fact]
    public async Task SecureTokenStore_ShouldRoundTripEncryptedSession()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.session");

        try
        {
            PasswordVaultSecureTokenStore store = new(tempFile);
            AuthSession session = new()
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token",
                ExpiresAt = DateTimeOffset.Parse("2026-05-12T09:00:00Z"),
                Provider = "husqvarna",
                UserId = "user-1"
            };

            (await store.SaveAsync(session, CancellationToken.None)).IsSuccess.Should().BeTrue();

            OperationResult<AuthSession?> loadResult = await store.LoadAsync(CancellationToken.None);
            loadResult.IsSuccess.Should().BeTrue();
            loadResult.Value.Should().NotBeNull();
            loadResult.Value!.AccessToken.Should().Be("access-token");
            loadResult.Value.RefreshToken.Should().Be("refresh-token");
            loadResult.Value.Provider.Should().Be("husqvarna");
            loadResult.Value.UserId.Should().Be("user-1");
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task ApplicationSecretStore_ShouldRoundTripEncryptedSecret()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.secret");

        try
        {
            PasswordVaultApplicationSecretStore store = new(tempFile);

            (await store.SaveAsync("super-secret", CancellationToken.None)).IsSuccess.Should().BeTrue();

            OperationResult<string?> loadResult = await store.LoadAsync(CancellationToken.None);
            loadResult.IsSuccess.Should().BeTrue();
            loadResult.Value.Should().Be("super-secret");

            (await store.DeleteAsync(CancellationToken.None)).IsSuccess.Should().BeTrue();
            (await store.LoadAsync(CancellationToken.None)).Value.Should().BeNull();
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}
