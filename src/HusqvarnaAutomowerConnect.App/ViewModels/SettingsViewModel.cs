using CommunityToolkit.Mvvm.ComponentModel;
using HusqvarnaAutomowerConnect.Core.Errors;
using HusqvarnaAutomowerConnect.Core.Interfaces;
using HusqvarnaAutomowerConnect.Core.Models;

namespace HusqvarnaAutomowerConnect.App.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IAppSettingsStore appSettingsStore;
    private readonly IApplicationSecretStore applicationSecretStore;

    [ObservableProperty]
    private string title = "Paramètres";

    [ObservableProperty]
    private string applicationKey = string.Empty;

    [ObservableProperty]
    private string redirectUri = "http://localhost";

    [ObservableProperty]
    private int refreshIntervalSeconds = AppSettings.DefaultRefreshIntervalSeconds;

    [ObservableProperty]
    private string minimumLogLevel = "Information";

    [ObservableProperty]
    private string applicationSecret = string.Empty;

    [ObservableProperty]
    private bool hasApplicationSecret;

    [ObservableProperty]
    private string secretStatusMessage = "Aucun secret d'application local n'est enregistré.";

    [ObservableProperty]
    private string statusMessage = "Aucune configuration chargée.";

    [ObservableProperty]
    private string validationMessage = string.Empty;

    public SettingsViewModel(
        IAppSettingsStore appSettingsStore,
        IApplicationSecretStore applicationSecretStore)
    {
        this.appSettingsStore = appSettingsStore ?? throw new ArgumentNullException(nameof(appSettingsStore));
        this.applicationSecretStore = applicationSecretStore ?? throw new ArgumentNullException(nameof(applicationSecretStore));
    }

    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        OperationResult<AppSettings> result = await appSettingsStore.LoadAsync(cancellationToken);
        if (!result.IsSuccess)
        {
            StatusMessage = result.Error?.UserMessage ?? "Impossible de charger les paramètres.";
            return;
        }

        Apply(result.Value!);
        await LoadSecretStatusAsync(cancellationToken);
        StatusMessage = "Paramètres locaux chargés.";
    }

    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        AppSettings settings = BuildSettings();
        IReadOnlyList<string> validationErrors = settings.Validate();
        if (validationErrors.Count > 0)
        {
            ValidationMessage = string.Join(" ", validationErrors);
            StatusMessage = "Corrigez les erreurs avant l'enregistrement.";
            return;
        }

        ValidationMessage = string.Empty;
        OperationResult saveResult = await appSettingsStore.SaveAsync(settings, cancellationToken);
        if (!saveResult.IsSuccess)
        {
            StatusMessage = saveResult.Error?.UserMessage ?? "L'enregistrement a échoué.";
            return;
        }

        if (!string.IsNullOrWhiteSpace(ApplicationSecret))
        {
            OperationResult secretResult = await applicationSecretStore.SaveAsync(ApplicationSecret, cancellationToken);
            if (!secretResult.IsSuccess)
            {
                StatusMessage = secretResult.Error?.UserMessage ?? "Le secret local n'a pas pu être enregistré.";
                return;
            }

            ApplicationSecret = string.Empty;
            HasApplicationSecret = true;
            SecretStatusMessage = "Secret d'application enregistré dans le stockage sécurisé Windows.";
        }

        StatusMessage = "Paramètres enregistrés.";
    }

    public async Task DeleteSecretAsync(CancellationToken cancellationToken)
    {
        OperationResult result = await applicationSecretStore.DeleteAsync(cancellationToken);
        if (!result.IsSuccess)
        {
            StatusMessage = result.Error?.UserMessage ?? "Impossible de supprimer le secret local.";
            return;
        }

        ApplicationSecret = string.Empty;
        HasApplicationSecret = false;
        SecretStatusMessage = "Aucun secret d'application local n'est enregistré.";
        StatusMessage = "Secret d'application supprimé.";
    }

    private void Apply(AppSettings settings)
    {
        ApplicationKey = settings.ApplicationKey;
        RedirectUri = settings.RedirectUri;
        RefreshIntervalSeconds = settings.RefreshIntervalSeconds;
        MinimumLogLevel = settings.MinimumLogLevel;
    }

    private async Task LoadSecretStatusAsync(CancellationToken cancellationToken)
    {
        OperationResult<string?> result = await applicationSecretStore.LoadAsync(cancellationToken);
        if (!result.IsSuccess)
        {
            HasApplicationSecret = false;
            SecretStatusMessage = result.Error?.UserMessage ?? "Impossible de vérifier le secret local.";
            return;
        }

        HasApplicationSecret = !string.IsNullOrWhiteSpace(result.Value);
        SecretStatusMessage = HasApplicationSecret
            ? "Secret d'application enregistré dans le stockage sécurisé Windows."
            : "Aucun secret d'application local n'est enregistré.";
    }

    private AppSettings BuildSettings() =>
        new()
        {
            ApplicationKey = ApplicationKey,
            RedirectUri = RedirectUri,
            RefreshIntervalSeconds = RefreshIntervalSeconds,
            MinimumLogLevel = MinimumLogLevel
        };
}
