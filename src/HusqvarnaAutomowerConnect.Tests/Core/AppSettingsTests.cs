using FluentAssertions;
using HusqvarnaAutomowerConnect.Core.Models;

namespace HusqvarnaAutomowerConnect.Tests.Core;

public sealed class AppSettingsTests
{
    [Fact]
    public void Validate_ShouldReturnError_WhenApplicationKeyIsMissing()
    {
        AppSettings settings = new()
        {
            ApplicationKey = "",
            RedirectUri = "http://localhost",
            RefreshIntervalSeconds = 60
        };

        IReadOnlyList<string> errors = settings.Validate();

        errors.Should().ContainSingle();
        errors[0].Should().Contain("clé d'application");
    }

    [Fact]
    public void Validate_ShouldReturnError_WhenRefreshIntervalIsTooShort()
    {
        AppSettings settings = new()
        {
            ApplicationKey = "demo-key",
            RedirectUri = "http://localhost",
            RefreshIntervalSeconds = 15
        };

        IReadOnlyList<string> errors = settings.Validate();

        errors.Should().ContainSingle();
        errors[0].Should().Contain("30 secondes");
    }
}

