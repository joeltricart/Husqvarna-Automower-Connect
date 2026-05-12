using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using HusqvarnaAutomowerConnect.Core.Errors;
using HusqvarnaAutomowerConnect.Core.Interfaces;
using HusqvarnaAutomowerConnect.Core.Models;

namespace HusqvarnaAutomowerConnect.App.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IMowerService mowerService;
    private readonly IClock clock;

    [ObservableProperty]
    private string title = "Tableau de bord";

    [ObservableProperty]
    private string statusMessage = "Aucun robot chargé.";

    [ObservableProperty]
    private string emptyStateMessage =
        "Aucun robot n'est affiché pour le moment. Actualisez la liste pour synchroniser les robots associés au compte.";

    [ObservableProperty]
    private string refreshInfo = "Dernière actualisation : jamais";

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private IReadOnlyList<MowerDashboardCard> cards = [];

    public DashboardViewModel(IMowerService mowerService, IClock clock)
    {
        this.mowerService = mowerService ?? throw new ArgumentNullException(nameof(mowerService));
        this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public Task LoadAsync(CancellationToken cancellationToken) => RefreshAsync(cancellationToken);

    public async Task RefreshAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        ErrorMessage = null;
        StatusMessage = "Chargement des robots en cours...";

        try
        {
            OperationResult<IReadOnlyList<Mower>> result = await mowerService.GetMowersAsync(cancellationToken);
            if (!result.IsSuccess)
            {
                Cards = [];
                ErrorMessage = result.Error?.UserMessage ?? "Impossible de charger les robots.";
                StatusMessage = "Le tableau de bord n'a pas pu être mis à jour.";
                RefreshInfo = BuildRefreshInfo();
                return;
            }

            IReadOnlyList<Mower> mowers = result.Value ?? [];
            Cards = mowers.Select(MapCard).ToArray();
            StatusMessage = Cards.Count switch
            {
                0 => "Aucun robot n'a été trouvé.",
                1 => "1 robot chargé.",
                _ => $"{Cards.Count} robots chargés."
            };
            EmptyStateMessage = Cards.Count == 0
                ? "Aucun robot n'est associé à cette session Husqvarna."
                : "Sélectionnez un robot pour afficher ses détails.";
            RefreshInfo = BuildRefreshInfo();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private string BuildRefreshInfo() =>
        $"Dernière actualisation : {clock.UtcNow.ToLocalTime():dd'/'MM'/'yyyy HH:mm}";

    private static MowerDashboardCard MapCard(Mower mower)
    {
        string name = string.IsNullOrWhiteSpace(mower.Name) ? "Robot sans nom" : mower.Name;
        string details = BuildDetails(mower);
        string statusLine = $"État : {DescribeState(mower.Status?.State ?? MowerState.Unknown)} | Activité : {DescribeActivity(mower.Status?.Activity ?? MowerActivity.Unknown)} | Mode : {DescribeMode(mower.Status?.Mode ?? MowerMode.Unknown)}";
        string batteryLine = mower.Battery?.LevelPercent is int levelPercent
            ? $"Batterie : {levelPercent} %"
            : "Batterie : non disponible";
        string connectivityLine = mower.Status?.Connected switch
        {
            true => "Connectivité : robot en ligne",
            false => "Connectivité : robot hors ligne",
            _ => "Connectivité : non disponible"
        };
        string locationLine = mower.Location is { } location
            ? string.Format(CultureInfo.CurrentCulture, "Position : {0:0.#####} / {1:0.#####}", location.Latitude, location.Longitude)
            : "Position : non disponible";
        string updatedLine = DescribeUpdatedAt(mower);
        string errorLine = mower.Status?.Error is { } error
            ? DescribeError(error)
            : "Erreur : aucune erreur signalée";

        return new MowerDashboardCard(
            mower.Id,
            name,
            details,
            statusLine,
            batteryLine,
            connectivityLine,
            locationLine,
            updatedLine,
            errorLine);
    }

    private static string BuildDetails(Mower mower)
    {
        List<string> details = [];

        if (!string.IsNullOrWhiteSpace(mower.Model))
        {
            details.Add(mower.Model);
        }

        if (!string.IsNullOrWhiteSpace(mower.SerialNumber))
        {
            details.Add($"N° de série {mower.SerialNumber}");
        }

        if (details.Count == 0)
        {
            details.Add("Modèle non disponible");
        }

        return string.Join(" | ", details);
    }

    private static string DescribeUpdatedAt(Mower mower)
    {
        DateTimeOffset? updatedAt = mower.LastUpdatedAt
            ?? mower.Status?.UpdatedAt
            ?? mower.Battery?.UpdatedAt
            ?? mower.Location?.UpdatedAt;

        return updatedAt.HasValue
            ? $"Dernière mise à jour : {updatedAt.Value.ToLocalTime():dd'/'MM'/'yyyy HH:mm}"
            : "Dernière mise à jour : non disponible";
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

        if (parts.Count == 0)
        {
            return "Erreur : non détaillée";
        }

        return $"Erreur : {string.Join(" - ", parts)}";
    }

    private static string DescribeState(MowerState state) =>
        state switch
        {
            MowerState.Ready => "Prêt",
            MowerState.InOperation => "En tonte",
            MowerState.Paused => "En pause",
            MowerState.Parked => "Stationné",
            MowerState.Error => "Erreur",
            MowerState.Offline => "Hors ligne",
            _ => "Inconnu"
        };

    private static string DescribeActivity(MowerActivity activity) =>
        activity switch
        {
            MowerActivity.Mowing => "Tonte",
            MowerActivity.GoingHome => "Retour station",
            MowerActivity.Charging => "Recharge",
            MowerActivity.Leaving => "Départ",
            MowerActivity.ParkedInChargingStation => "Stationné en charge",
            MowerActivity.Stopped => "Arrêté",
            _ => "Inconnue"
        };

    private static string DescribeMode(MowerMode mode) =>
        mode switch
        {
            MowerMode.MainArea => "Zone principale",
            MowerMode.SecondaryArea => "Zone secondaire",
            MowerMode.Home => "Retour station",
            MowerMode.Demo => "Démo",
            _ => "Inconnu"
        };
}
