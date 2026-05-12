using System.Text.RegularExpressions;

namespace HusqvarnaAutomowerConnect.Infrastructure.Security;

public static partial class SensitiveDataRedactor
{
    public static string Redact(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value ?? string.Empty;
        }

        string redacted = value;
        redacted = AuthorizationHeaderRegex().Replace(redacted, "Authorization: Bearer ***");
        redacted = TokenFieldRegex().Replace(redacted, "${prefix}***");
        redacted = ClientSecretRegex().Replace(redacted, "${prefix}***");
        return redacted;
    }

    [GeneratedRegex("Authorization\\s*:\\s*Bearer\\s+[^\\s\\\"']+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex AuthorizationHeaderRegex();

    [GeneratedRegex("(?<prefix>(access_token|refresh_token)\\s*[:=]\\s*)[^,\\s\\\"']+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex TokenFieldRegex();

    [GeneratedRegex("(?<prefix>client_secret\\s*[:=]\\s*)[^,\\s\\\"']+", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex ClientSecretRegex();
}
