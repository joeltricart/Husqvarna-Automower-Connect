using FluentAssertions;
using HusqvarnaAutomowerConnect.Core.Models;
using HusqvarnaAutomowerConnect.Infrastructure.Configuration;

namespace HusqvarnaAutomowerConnect.Tests.Infrastructure;

public sealed class LocalAppSettingsStoreTests
{
    [Fact]
    public async Task SaveAndLoadAsync_ShouldRoundTripNonSensitiveSettings()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");

        try
        {
            LocalAppSettingsStore store = new(tempFile);
            AppSettings settings = new()
            {
                ApplicationKey = "demo-key",
                RedirectUri = "http://localhost",
                RefreshIntervalSeconds = 60,
                MinimumLogLevel = "Warning"
            };

            var saveResult = await store.SaveAsync(settings, CancellationToken.None);
            var loadResult = await store.LoadAsync(CancellationToken.None);

            saveResult.IsSuccess.Should().BeTrue();
            loadResult.IsSuccess.Should().BeTrue();
            loadResult.Value.Should().BeEquivalentTo(settings);
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

