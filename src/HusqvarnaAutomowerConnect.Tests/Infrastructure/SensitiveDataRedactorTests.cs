using FluentAssertions;
using HusqvarnaAutomowerConnect.Infrastructure.Security;

namespace HusqvarnaAutomowerConnect.Tests.Infrastructure;

public sealed class SensitiveDataRedactorTests
{
    [Theory]
    [InlineData("Authorization: Bearer abcdef", "Authorization: Bearer ***")]
    [InlineData("access_token=abcdef", "access_token=***")]
    [InlineData("refresh_token: abcdef", "refresh_token: ***")]
    [InlineData("client_secret=abcdef", "client_secret=***")]
    public void Redact_ShouldMaskSensitiveFields(string input, string expected)
    {
        string result = SensitiveDataRedactor.Redact(input);

        result.Should().Be(expected);
    }
}
