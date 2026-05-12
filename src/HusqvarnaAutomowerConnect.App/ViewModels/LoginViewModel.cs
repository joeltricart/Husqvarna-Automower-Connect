using CommunityToolkit.Mvvm.ComponentModel;
using HusqvarnaAutomowerConnect.Core.Errors;
using HusqvarnaAutomowerConnect.Core.Interfaces;
using HusqvarnaAutomowerConnect.Core.Models;

namespace HusqvarnaAutomowerConnect.App.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAppSettingsStore appSettingsStore;
    private readonly IApplicationSecretStore applicationSecretStore;
    private readonly ISecureTokenStore secureTokenStore;
    private readonly IHusqvarnaAuthClient authClient;
    private readonly IOAuthCallbackListener callbackListener;
    private readonly IBrowserLauncher browserLauncher;

    [ObservableProperty]
    private string title = "Connexion Husqvarna";

    [ObservableProperty]
    private string helperText =
        "Connectez votre compte Husqvarna Automower Connect pour afficher vos robots.";

    [ObservableProperty]
    private string connectionState = "Session locale : inconnue";

    [ObservableProperty]
    private string secretState = "Secret d'application : inconnue";

    [ObservableProperty]
    private bool hasLocalSession;

    [ObservableProperty]
    private string localSessionDetail = "Aucun jeton local chargé.";

    [ObservableProperty]
    private bool isBusy;

    public LoginViewModel(
        IAppSettingsStore appSettingsStore,
        IApplicationSecretStore applicationSecretStore,
        ISecureTokenStore secureTokenStore,
        IHusqvarnaAuthClient authClient,
        IOAuthCallbackListener callbackListener,
        IBrowserLauncher browserLauncher)
    {
        this.appSettingsStore = appSettingsStore ?? throw new ArgumentNullException(nameof(appSettingsStore));
        this.applicationSecretStore = applicationSecretStore ?? throw new ArgumentNullException(nameof(applicationSecretStore));
        this.secureTokenStore = secureTokenStore ?? throw new ArgumentNullException(nameof(secureTokenStore));
        this.authClient = authClient ?? throw new ArgumentNullException(nameof(authClient));
        this.callbackListener = callbackListener ?? throw new ArgumentNullException(nameof(callbackListener));
        this.browserLauncher = browserLauncher ?? throw new ArgumentNullException(nameof(browserLauncher));
    }

    public async Task RefreshAsync(CancellationToken cancellationToken)
    {
        OperationResult<AuthSession?> sessionResult = await secureTokenStore.LoadAsync(cancellationToken);
        OperationResult<string?> secretResult = await applicationSecretStore.LoadAsync(cancellationToken);

        if (!sessionResult.IsSuccess)
        {
            HasLocalSession = false;
            ConnectionState = "Session locale : indisponible";
            LocalSessionDetail = sessionResult.Error?.UserMessage ?? "Impossible de charger la session locale.";
        }
        else
        {
            AuthSession? session = sessionResult.Value;
            HasLocalSession = session is not null && session.HasRefreshToken;
            ConnectionState = HasLocalSession
                ? "Session locale : disponible"
                : "Session locale : absente";
            LocalSessionDetail = HasLocalSession
                ? "Un jeton de renouvellement est présent dans le stockage sécurisé Windows."
                : "Aucun jeton n'est stocké localement.";
        }

        if (!secretResult.IsSuccess)
        {
            SecretState = secretResult.Error?.UserMessage ?? "Impossible de vérifier le secret local.";
        }
        else
        {
            SecretState = secretResult.Value is { Length: > 0 }
                ? "Secret d'application : enregistré"
                : "Secret d'application : absent";
        }
    }

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ConnectionState = "Connexion en cours...";

        try
        {
            OperationResult<AppSettings> settingsResult = await appSettingsStore.LoadAsync(cancellationToken);
            if (!settingsResult.IsSuccess)
            {
                ConnectionState = settingsResult.Error?.UserMessage ?? "Impossible de charger la configuration.";
                return;
            }

            AppSettings settings = settingsResult.Value!;
            IReadOnlyList<string> validationErrors = settings.Validate();
            if (validationErrors.Count > 0)
            {
                ConnectionState = validationErrors[0];
                return;
            }

            OperationResult<string?> secretResult = await applicationSecretStore.LoadAsync(cancellationToken);
            if (!secretResult.IsSuccess)
            {
                ConnectionState = secretResult.Error?.UserMessage ?? "Impossible de charger le secret d'application.";
                return;
            }

            string? clientSecret = secretResult.Value;
            if (string.IsNullOrWhiteSpace(clientSecret))
            {
                ConnectionState = "Configuration Husqvarna incomplète. Ajoutez le secret d'application dans Paramètres.";
                return;
            }

            string state = Guid.NewGuid().ToString("N");
            Uri authorizationUri = authClient.BuildAuthorizationUri(settings, state, null);

            using CancellationTokenSource loginCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            loginCancellation.CancelAfter(TimeSpan.FromMinutes(2));
            Task<OperationResult<OAuthCallbackResult>> callbackTask = callbackListener.WaitForCallbackAsync(
                new Uri(settings.RedirectUri),
                state,
                loginCancellation.Token);

            OperationResult browserResult = await browserLauncher.OpenAsync(authorizationUri, cancellationToken);
            if (!browserResult.IsSuccess)
            {
                loginCancellation.Cancel();
                ConnectionState = browserResult.Error?.UserMessage ?? "Impossible d'ouvrir le navigateur par défaut.";
                return;
            }

            OperationResult<OAuthCallbackResult> callbackResult = await callbackTask;
            if (!callbackResult.IsSuccess)
            {
                ConnectionState = callbackResult.Error?.UserMessage ?? "La connexion a échoué.";
                return;
            }

            OAuthCallbackResult callback = callbackResult.Value!;
            if (!string.IsNullOrWhiteSpace(callback.Error))
            {
                ConnectionState = callback.ErrorDescription ?? callback.Error;
                return;
            }

            if (string.IsNullOrWhiteSpace(callback.Code))
            {
                ConnectionState = "Le code d'autorisation est manquant.";
                return;
            }

            OperationResult<AuthSession> exchangeResult = await authClient.ExchangeAuthorizationCodeAsync(
                callback.Code,
                settings,
                clientSecret,
                cancellationToken);

            if (!exchangeResult.IsSuccess)
            {
                ConnectionState = exchangeResult.Error?.UserMessage ?? "L'échange du code a échoué.";
                return;
            }

            OperationResult saveResult = await secureTokenStore.SaveAsync(exchangeResult.Value!, cancellationToken);
            if (!saveResult.IsSuccess)
            {
                ConnectionState = saveResult.Error?.UserMessage ?? "Impossible de stocker la session localement.";
                return;
            }

            await RefreshAsync(cancellationToken);
            ConnectionState = "Connexion réussie.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken)
    {
        string? revokeWarning = null;
        OperationResult<AuthSession?> loadResult = await secureTokenStore.LoadAsync(cancellationToken);
        if (loadResult.IsSuccess && loadResult.Value?.AccessToken is { Length: > 0 } accessToken)
        {
            OperationResult revokeResult = await authClient.RevokeAsync(accessToken, cancellationToken);
            if (!revokeResult.IsSuccess)
            {
                revokeWarning = revokeResult.Error?.UserMessage ?? "La révocation distante a échoué.";
            }
        }

        OperationResult deleteResult = await secureTokenStore.DeleteAsync(cancellationToken);
        if (!deleteResult.IsSuccess)
        {
            ConnectionState = deleteResult.Error?.UserMessage ?? "La déconnexion locale a échoué.";
            return;
        }

        await RefreshAsync(cancellationToken);
        ConnectionState = revokeWarning is null
            ? "Session locale supprimée."
            : $"Session locale supprimée. {revokeWarning}";
    }
}
