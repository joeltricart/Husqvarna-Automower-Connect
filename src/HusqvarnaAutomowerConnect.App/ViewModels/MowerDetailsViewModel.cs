using CommunityToolkit.Mvvm.ComponentModel;
using HusqvarnaAutomowerConnect.Core.Errors;
using HusqvarnaAutomowerConnect.Core.Interfaces;
using HusqvarnaAutomowerConnect.Core.Models;

namespace HusqvarnaAutomowerConnect.App.ViewModels;

public partial class MowerDetailsViewModel : ObservableObject
{
    private readonly IMowerService mowerService;
    private string? currentMowerId;

    [ObservableProperty]
    private string title = "Détail robot";

    [ObservableProperty]
    private string commandHint =
        "Les commandes restent désactivées tant que les capacités du robot n'ont pas été confirmées par l'API.";

    [ObservableProperty]
    private string stateSummary = "Aucun robot sélectionné.";

    [ObservableProperty]
    private string statusMessage = "Aucune action en attente.";

    [ObservableProperty]
    private string mowerName = "Robot non chargé";

    [ObservableProperty]
    private string mowerIdentifier = string.Empty;

    [ObservableProperty]
    private string batterySummary = "Batterie : non disponible";

    [ObservableProperty]
    private string connectivitySummary = "Connectivité : non disponible";

    [ObservableProperty]
    private string locationSummary = "Position : non disponible";

    [ObservableProperty]
    private string errorSummary = "Erreur : aucune erreur signalée";

    [ObservableProperty]
    private int selectedDurationMinutes = 30;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool hasMower;

    [ObservableProperty]
    private bool canPause;

    [ObservableProperty]
    private bool canParkUntilNextSchedule;

    [ObservableProperty]
    private bool canParkUntilFurtherNotice;

    [ObservableProperty]
    private bool canResumeSchedule;

    [ObservableProperty]
    private bool canStartForDuration;

    public MowerDetailsViewModel(IMowerService mowerService)
    {
        this.mowerService = mowerService ?? throw new ArgumentNullException(nameof(mowerService));
    }

    public async Task LoadAsync(CancellationToken cancellationToken, string? mowerId = null)
    {
        IsLoading = true;
        StatusMessage = "Chargement du robot en cours...";

        try
        {
            Mower? mower = null;

            if (!string.IsNullOrWhiteSpace(mowerId))
            {
                OperationResult<Mower> mowerResult = await mowerService.GetMowerAsync(mowerId, cancellationToken);
                if (!mowerResult.IsSuccess)
                {
                    ResetUnavailable(mowerResult.Error?.UserMessage ?? "Impossible de charger le robot.");
                    return;
                }

                mower = mowerResult.Value;
            }
            else
            {
                OperationResult<IReadOnlyList<Mower>> mowersResult = await mowerService.GetMowersAsync(cancellationToken);
                if (!mowersResult.IsSuccess)
                {
                    ResetUnavailable(mowersResult.Error?.UserMessage ?? "Impossible de charger la liste des robots.");
                    return;
                }

                mower = mowersResult.Value?.FirstOrDefault();
                if (mower is null)
                {
                    ResetUnavailable("Aucun robot n'est disponible pour afficher son détail.");
                    return;
                }
            }

            if (mower is null)
            {
                ResetUnavailable("Aucun robot n'a été trouvé.");
                return;
            }

            ApplyMower(mower);
            StatusMessage = "Robot chargé.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public Task PauseAsync(CancellationToken cancellationToken) =>
        ExecuteCommandAsync(new MowerCommand { Type = MowerCommandType.Pause }, cancellationToken);

    public Task ResumeScheduleAsync(CancellationToken cancellationToken) =>
        ExecuteCommandAsync(new MowerCommand { Type = MowerCommandType.ResumeSchedule }, cancellationToken);

    public Task ParkUntilNextScheduleAsync(CancellationToken cancellationToken) =>
        ExecuteCommandAsync(new MowerCommand { Type = MowerCommandType.ParkUntilNextSchedule }, cancellationToken);

    public Task ParkUntilFurtherNoticeAsync(CancellationToken cancellationToken) =>
        ExecuteCommandAsync(new MowerCommand { Type = MowerCommandType.ParkUntilFurtherNotice }, cancellationToken);

    public Task StartForDurationAsync(CancellationToken cancellationToken) =>
        ExecuteCommandAsync(new MowerCommand
        {
            Type = MowerCommandType.StartForDuration,
            Duration = TimeSpan.FromMinutes(SelectedDurationMinutes)
        }, cancellationToken);

    private async Task ExecuteCommandAsync(MowerCommand command, CancellationToken cancellationToken)
    {
        if (!HasMower || string.IsNullOrWhiteSpace(currentMowerId))
        {
            StatusMessage = "Aucun robot n'est chargé.";
            return;
        }

        StatusMessage = "Envoi de la commande en cours...";
        ErrorSummary = "Erreur : aucune erreur signalée";

        OperationResult<CommandResult> result = await mowerService.SendCommandAsync(currentMowerId, command, cancellationToken);
        if (!result.IsSuccess)
        {
            StatusMessage = "La commande n'a pas pu être envoyée.";
            ErrorSummary = result.Error?.UserMessage ?? "Erreur de commande inconnue.";
            return;
        }

        StatusMessage = result.Value?.Message ?? "Commande envoyée.";
        if (result.Value?.ShouldRefresh == true)
        {
            await LoadAsync(cancellationToken, currentMowerId);
        }
    }

    private void ApplyMower(Mower mower)
    {
        currentMowerId = mower.Id;
        HasMower = true;
        MowerName = string.IsNullOrWhiteSpace(mower.Name) ? "Robot sans nom" : mower.Name;
        MowerIdentifier = string.IsNullOrWhiteSpace(mower.Id) ? "Identifiant non disponible" : $"Identifiant : {mower.Id}";
        StateSummary = BuildStateSummary(mower);
        BatterySummary = mower.Battery?.LevelPercent is int level
            ? $"Batterie : {level} %"
            : "Batterie : non disponible";
        ConnectivitySummary = mower.Status?.Connected switch
        {
            true => "Connectivité : robot en ligne",
            false => "Connectivité : robot hors ligne",
            _ => "Connectivité : non disponible"
        };
        LocationSummary = mower.Location is { } location
            ? $"Position : {location.Latitude:0.#####} / {location.Longitude:0.#####}"
            : "Position : non disponible";
        ErrorSummary = mower.Status?.Error is { } error ? DescribeError(error) : "Erreur : aucune erreur signalée";
        CanPause = mower.Capabilities.CanPause;
        CanParkUntilNextSchedule = mower.Capabilities.CanParkUntilNextSchedule;
        CanParkUntilFurtherNotice = mower.Capabilities.CanParkUntilFurtherNotice;
        CanResumeSchedule = mower.Capabilities.CanResumeSchedule;
        CanStartForDuration = mower.Capabilities.CanStartForDuration;
        CommandHint = BuildCommandHint(mower);
    }

    private void ResetUnavailable(string message)
    {
        HasMower = false;
        currentMowerId = null;
        MowerName = "Robot non chargé";
        MowerIdentifier = string.Empty;
        StateSummary = "Aucun robot sélectionné.";
        BatterySummary = "Batterie : non disponible";
        ConnectivitySummary = "Connectivité : non disponible";
        LocationSummary = "Position : non disponible";
        ErrorSummary = "Erreur : aucune erreur signalée";
        CanPause = false;
        CanParkUntilNextSchedule = false;
        CanParkUntilFurtherNotice = false;
        CanResumeSchedule = false;
        CanStartForDuration = false;
        CommandHint = "Les commandes restent désactivées tant qu'aucun robot n'est chargé.";
        StatusMessage = message;
    }

    private static string BuildStateSummary(Mower mower)
    {
        string state = mower.Status?.State switch
        {
            MowerState.Ready => "Prêt",
            MowerState.InOperation => "En tonte",
            MowerState.Paused => "En pause",
            MowerState.Parked => "Stationné",
            MowerState.Error => "Erreur",
            MowerState.Offline => "Hors ligne",
            _ => "Inconnu"
        };

        string activity = mower.Status?.Activity switch
        {
            MowerActivity.Mowing => "Tonte",
            MowerActivity.GoingHome => "Retour station",
            MowerActivity.Charging => "Recharge",
            MowerActivity.Leaving => "Départ",
            MowerActivity.ParkedInChargingStation => "Stationné en charge",
            MowerActivity.Stopped => "Arrêté",
            _ => "Inconnue"
        };

        string mode = mower.Status?.Mode switch
        {
            MowerMode.MainArea => "Zone principale",
            MowerMode.SecondaryArea => "Zone secondaire",
            MowerMode.Home => "Retour station",
            MowerMode.Demo => "Démo",
            _ => "Inconnu"
        };

        return $"État : {state} | Activité : {activity} | Mode : {mode}";
    }

    private static string BuildCommandHint(Mower mower)
    {
        List<string> parts = [];

        if (mower.Capabilities.CanPause)
        {
            parts.Add("Pause");
        }

        if (mower.Capabilities.CanResumeSchedule)
        {
            parts.Add("Reprise du planning");
        }

        if (mower.Capabilities.CanParkUntilNextSchedule)
        {
            parts.Add("Retour station jusqu'à la prochaine session");
        }

        if (mower.Capabilities.CanParkUntilFurtherNotice)
        {
            parts.Add("Retour station jusqu'à nouvel ordre");
        }

        if (mower.Capabilities.CanStartForDuration)
        {
            parts.Add("Tonte temporaire");
        }

        return parts.Count == 0
            ? "Aucune commande n'est disponible pour ce robot dans son état actuel."
            : $"Commandes disponibles : {string.Join(", ", parts)}.";
    }

    private static string DescribeError(MowerError error)
    {
        List<string> parts = [];

        if (!string.IsNullOrWhiteSpace(error.Code))
        {
            parts.Add($"code {error.Code}");
        }

        if (!string.IsNullOrWhiteSpace(error.Message))
        {
            parts.Add(error.Message);
        }

        return parts.Count == 0
            ? "Erreur : non détaillée"
            : $"Erreur : {string.Join(" - ", parts)}";
    }
}
