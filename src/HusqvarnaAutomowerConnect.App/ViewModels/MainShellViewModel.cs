using CommunityToolkit.Mvvm.ComponentModel;

namespace HusqvarnaAutomowerConnect.App.ViewModels;

public partial class MainShellViewModel : ObservableObject
{
    [ObservableProperty]
    private string currentSectionTitle = "Connexion";

    [ObservableProperty]
    private string statusMessage =
        "La V1 affiche le shell, les paramètres locaux, la connexion Husqvarna et l'état de session.";

    public string ApplicationTitle => "Husqvarna Automower Connect";

    public void UpdateSection(string sectionTitle) => CurrentSectionTitle = sectionTitle;
}
