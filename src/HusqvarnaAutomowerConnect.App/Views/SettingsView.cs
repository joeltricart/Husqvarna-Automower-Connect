using HusqvarnaAutomowerConnect.App.Diagnostics;
using HusqvarnaAutomowerConnect.App.ViewModels;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Button = Microsoft.UI.Xaml.Controls.Button;
using Orientation = Microsoft.UI.Xaml.Controls.Orientation;
using ScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility;
using ScrollViewer = Microsoft.UI.Xaml.Controls.ScrollViewer;
using StackPanel = Microsoft.UI.Xaml.Controls.StackPanel;
using TextBlock = Microsoft.UI.Xaml.Controls.TextBlock;
using UserControl = Microsoft.UI.Xaml.Controls.UserControl;

namespace HusqvarnaAutomowerConnect.App.Views;

public sealed class SettingsView : UserControl
{
    private readonly SettingsViewModel viewModel;
    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    public SettingsView(SettingsViewModel viewModel)
    {
        AppDiagnostics.Log("SettingsView: constructeur - début.");
        this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        RenderState();
        AppDiagnostics.Log("SettingsView: constructeur - fin.");
    }

    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        AppDiagnostics.Log("SettingsView: LoadAsync - début.");
        await viewModel.LoadAsync(cancellationToken);
        RenderState();
        AppDiagnostics.Log("SettingsView: LoadAsync - fin.");
    }

    private void RenderState()
    {
        if (!dispatcherQueue.HasThreadAccess)
        {
            dispatcherQueue.TryEnqueue(RenderState);
            return;
        }

        AppDiagnostics.Log("SettingsView: RenderState - début.");

        ScrollViewer scrollViewer = new()
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        StackPanel root = new()
        {
            Padding = new Thickness(24),
            Spacing = 12
        };

        root.Children.Add(new TextBlock
        {
            Text = viewModel.Title,
            FontSize = 24,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
        });

        root.Children.Add(new TextBlock
        {
            Text = "Les paramètres sont affichés ici. Utilisez le bouton Modifier pour les saisir dans une fenêtre dédiée.",
            TextWrapping = TextWrapping.Wrap
        });

        root.Children.Add(CreateDisplayBlock(
            "Clé d'application",
            string.IsNullOrWhiteSpace(viewModel.ApplicationKey) ? "Aucune clé locale." : viewModel.ApplicationKey));
        root.Children.Add(CreateDisplayBlock(
            "URI de redirection",
            string.IsNullOrWhiteSpace(viewModel.RedirectUri) ? "Aucune URI locale." : viewModel.RedirectUri));
        root.Children.Add(CreateDisplayBlock(
            "Intervalle de rafraîchissement (secondes)",
            viewModel.RefreshIntervalSeconds.ToString()));
        root.Children.Add(CreateDisplayBlock("Niveau de log", viewModel.MinimumLogLevel));
        root.Children.Add(CreateDisplayBlock("Secret d'application", viewModel.SecretStatusMessage));

        StackPanel buttons = new()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8
        };

        Button editButton = new()
        {
            Content = "Modifier"
        };
        editButton.Click += async (_, _) => await OpenEditorAsync();

        Button reloadButton = new()
        {
            Content = "Recharger"
        };
        reloadButton.Click += async (_, _) =>
        {
            await LoadAsync(CancellationToken.None);
        };

        Button deleteSecretButton = new()
        {
            Content = "Supprimer le secret local"
        };
        deleteSecretButton.Click += async (_, _) =>
        {
            await viewModel.DeleteSecretAsync(CancellationToken.None);
            RenderState();
        };

        buttons.Children.Add(editButton);
        buttons.Children.Add(reloadButton);
        buttons.Children.Add(deleteSecretButton);

        root.Children.Add(buttons);
        root.Children.Add(CreateDisplayBlock("Statut", viewModel.StatusMessage));
        root.Children.Add(CreateDisplayBlock("Validation", viewModel.ValidationMessage));

        scrollViewer.Content = root;
        Content = scrollViewer;

        AppDiagnostics.Log("SettingsView: RenderState - fin.");
    }

    private static StackPanel CreateDisplayBlock(string label, string value)
    {
        StackPanel panel = new()
        {
            Spacing = 4
        };

        panel.Children.Add(new TextBlock
        {
            Text = label
        });

        panel.Children.Add(new TextBlock
        {
            Text = value,
            IsTextScaleFactorEnabled = false,
            TextWrapping = TextWrapping.Wrap
        });

        return panel;
    }

    private async Task OpenEditorAsync()
    {
        AppDiagnostics.Log("SettingsView: OpenEditorAsync.");

        SettingsEditDialog dialog = new(
            viewModel.ApplicationKey,
            viewModel.RedirectUri,
            viewModel.RefreshIntervalSeconds,
            viewModel.MinimumLogLevel);

        if (!dialog.ShowDialog())
        {
            return;
        }

        viewModel.ApplicationKey = dialog.ApplicationKey;
        viewModel.RedirectUri = dialog.RedirectUri;
        viewModel.RefreshIntervalSeconds = dialog.RefreshIntervalSeconds;
        viewModel.MinimumLogLevel = dialog.MinimumLogLevel;
        viewModel.ApplicationSecret = dialog.ApplicationSecret ?? string.Empty;

        await viewModel.SaveAsync(CancellationToken.None);
        RenderState();
    }
}
