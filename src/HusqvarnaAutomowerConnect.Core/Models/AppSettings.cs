namespace HusqvarnaAutomowerConnect.Core.Models;

public sealed record AppSettings
{
    public const int MinimumRefreshIntervalSeconds = 30;
    public const int DefaultRefreshIntervalSeconds = 60;

    public string ApplicationKey { get; init; } = string.Empty;

    public string RedirectUri { get; init; } = "http://localhost";

    public int RefreshIntervalSeconds { get; init; } = DefaultRefreshIntervalSeconds;

    public string MinimumLogLevel { get; init; } = "Information";

    public IReadOnlyList<string> Validate()
    {
        List<string> errors = [];

        if (string.IsNullOrWhiteSpace(ApplicationKey))
        {
            errors.Add("La clé d'application Husqvarna est requise.");
        }

        if (!Uri.TryCreate(RedirectUri, UriKind.Absolute, out _))
        {
            errors.Add("L'URI de redirection doit être une URI absolue valide.");
        }

        if (RefreshIntervalSeconds < MinimumRefreshIntervalSeconds)
        {
            errors.Add("L'intervalle de rafraîchissement doit être au minimum de 30 secondes.");
        }

        return errors;
    }
}

