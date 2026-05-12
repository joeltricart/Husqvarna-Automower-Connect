namespace HusqvarnaAutomowerConnect.Infrastructure.Api;

public static class OfficialApiContract
{
    public const string AuthenticationBaseUrl = "https://api.authentication.husqvarnagroup.dev/v1/";
    public const string AutomowerBaseUrl = "https://api.amc.husqvarna.dev/v1/";
    public const string ApplicationKeyHeader = "X-Api-Key";
    public const string AuthorizationHeader = "Authorization";
    public const string AuthorizationProviderHeader = "Authorization-Provider";
    public const string AuthorizationProviderValue = "husqvarna";
    public const string JsonApiContentType = "application/vnd.api+json";

    public static class Authentication
    {
        public const string AuthorizePath = "oauth2/authorize";
        public const string TokenPath = "oauth2/token";
        public const string RevokePath = "oauth2/revoke";
    }

    public static class Automower
    {
        public const string MowersPath = "mowers";
    }
}
