using HusqvarnaAutomowerConnect.App.ViewModels;
using HusqvarnaAutomowerConnect.App.Views;
using HusqvarnaAutomowerConnect.App.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Border = Microsoft.UI.Xaml.Controls.Border;
using Button = Microsoft.UI.Xaml.Controls.Button;
using ContentControl = Microsoft.UI.Xaml.Controls.ContentControl;
using Grid = Microsoft.UI.Xaml.Controls.Grid;
using GridLength = Microsoft.UI.Xaml.GridLength;
using GridUnitType = Microsoft.UI.Xaml.GridUnitType;
using HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment;
using Orientation = Microsoft.UI.Xaml.Controls.Orientation;
using RowDefinition = Microsoft.UI.Xaml.Controls.RowDefinition;
using StackPanel = Microsoft.UI.Xaml.Controls.StackPanel;
using TextBlock = Microsoft.UI.Xaml.Controls.TextBlock;
using Thickness = Microsoft.UI.Xaml.Thickness;
using TextWrapping = Microsoft.UI.Xaml.TextWrapping;

namespace HusqvarnaAutomowerConnect.App;

public sealed class MainWindow : Window
{
    private readonly IServiceProvider serviceProvider;
    private readonly MainShellViewModel shellViewModel = new();
    private readonly LoginView loginView;
    private readonly ContentControl contentHost = new();
    private readonly Button loginButton = new() { Content = "Connexion" };
    private readonly Button dashboardButton = new() { Content = "Tableau de bord" };
    private readonly Button settingsButton = new() { Content = "Paramètres" };
    private readonly Button mowerButton = new() { Content = "Robot" };
    private DashboardView? dashboardView;
    private SettingsView? settingsView;
    private MowerDetailsView? mowerDetailsView;
    private string? selectedMowerId;
    private bool initialized;
    private readonly System.Threading.Timer heartbeatTimer;

    public MainWindow(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        this.serviceProvider = serviceProvider;
        AppDiagnostics.Log("Constructeur MainWindow.");

        Title = shellViewModel.ApplicationTitle;

        LoginViewModel loginViewModel = serviceProvider.GetRequiredService<LoginViewModel>();
        loginView = new LoginView(loginViewModel);

        RootLayout();
        Activated += MainWindow_Activated;
        heartbeatTimer = new System.Threading.Timer(
            _ => AppDiagnostics.Log("Heartbeat fenêtre principale."),
            null,
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(5));
    }

    private void RootLayout()
    {
        Grid root = new();
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        Border header = new()
        {
            Padding = new Thickness(24, 20, 24, 16),
            Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent)
        };

        StackPanel headerPanel = new() { Spacing = 4 };
        headerPanel.Children.Add(new TextBlock
        {
            Text = shellViewModel.ApplicationTitle,
            FontSize = 26,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
        });
        headerPanel.Children.Add(new TextBlock
        {
            Text = shellViewModel.StatusMessage,
            TextWrapping = TextWrapping.Wrap
        });
        header.Child = headerPanel;

        StackPanel navigationBar = new()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(24, 0, 24, 8),
            Spacing = 8
        };

        loginButton.Click += async (_, _) => await NavigateAsync("login");
        dashboardButton.Click += async (_, _) => await NavigateAsync("dashboard");
        settingsButton.Click += async (_, _) => await NavigateAsync("settings");
        mowerButton.Click += async (_, _) => await NavigateAsync("mower");

        navigationBar.Children.Add(loginButton);
        navigationBar.Children.Add(dashboardButton);
        navigationBar.Children.Add(settingsButton);
        navigationBar.Children.Add(mowerButton);

        StackPanel contentPanel = new()
        {
            Spacing = 12
        };
        contentPanel.Children.Add(navigationBar);
        contentPanel.Children.Add(contentHost);

        Grid.SetRow(header, 0);
        Grid.SetRow(contentPanel, 1);
        root.Children.Add(header);
        root.Children.Add(contentPanel);

        Content = root;
    }

    private async void MainWindow_Activated(object? sender, WindowActivatedEventArgs args)
    {
        AppDiagnostics.Log($"Activated | {args.WindowActivationState}");
        if (initialized || args.WindowActivationState == WindowActivationState.Deactivated)
        {
            return;
        }

        initialized = true;
        Activated -= MainWindow_Activated;

        try
        {
            await NavigateAsync("login");
        }
        catch (Exception exception)
        {
            AppDiagnostics.LogException("MainWindow_Activated", exception);
            shellViewModel.StatusMessage = "Le démarrage a échoué.";
            contentHost.Content = null;
        }
    }

    private async Task NavigateAsync(string target)
    {
        AppDiagnostics.Log($"NavigateAsync | {target}");
        UpdateNavigationState(target);

        try
        {
            switch (target)
            {
                case "login":
                    shellViewModel.UpdateSection("Connexion");
                    await ShowLoginAsync();
                    break;
                case "dashboard":
                    shellViewModel.UpdateSection("Tableau de bord");
                    await ShowDashboardAsync();
                    break;
                case "settings":
                    shellViewModel.UpdateSection("Paramètres");
                    await ShowSettingsAsync();
                    break;
                case "mower":
                    shellViewModel.UpdateSection("Robot");
                    await ShowMowerDetailsAsync();
                    break;
            }
        }
        catch (Exception exception)
        {
            AppDiagnostics.LogException($"NavigateAsync:{target}", exception);
            shellViewModel.StatusMessage = "Une erreur est survenue dans l'interface.";
            contentHost.Content = null;
        }
    }

    private void UpdateNavigationState(string activeTarget)
    {
        loginButton.IsEnabled = activeTarget != "login";
        dashboardButton.IsEnabled = activeTarget != "dashboard";
        settingsButton.IsEnabled = activeTarget != "settings";
        mowerButton.IsEnabled = activeTarget != "mower";
    }

    private async Task ShowLoginAsync()
    {
        AppDiagnostics.Log("ShowLoginAsync.");
        contentHost.Content = loginView;
        await loginView.RefreshAsync(CancellationToken.None);
    }

    private async Task ShowDashboardAsync()
    {
        AppDiagnostics.Log("ShowDashboardAsync.");
        dashboardView ??= new DashboardView(
            serviceProvider.GetRequiredService<DashboardViewModel>(),
            OpenMowerDetailsAsync);

        contentHost.Content = dashboardView;
        await dashboardView.RefreshAsync(CancellationToken.None);
    }

    private async Task ShowSettingsAsync()
    {
        AppDiagnostics.Log("ShowSettingsAsync.");
        settingsView ??= new SettingsView(serviceProvider.GetRequiredService<SettingsViewModel>());
        AppDiagnostics.Log("ShowSettingsAsync: vue créée.");
        contentHost.Content = settingsView;
        AppDiagnostics.Log("ShowSettingsAsync: vue assignée.");
        await settingsView.LoadAsync(CancellationToken.None);
        AppDiagnostics.Log("ShowSettingsAsync: chargement terminé.");
    }

    private Task ShowMowerDetailsAsync()
    {
        AppDiagnostics.Log("ShowMowerDetailsAsync.");
        mowerDetailsView ??= new MowerDetailsView(serviceProvider.GetRequiredService<MowerDetailsViewModel>());
        contentHost.Content = mowerDetailsView;
        return mowerDetailsView.RefreshAsync(CancellationToken.None, selectedMowerId);
    }

    private async Task OpenMowerDetailsAsync(string mowerId)
    {
        AppDiagnostics.Log($"OpenMowerDetailsAsync | {mowerId}");
        selectedMowerId = mowerId;
        await NavigateAsync("mower");
    }
}
