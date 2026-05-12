using HusqvarnaAutomowerConnect.App.Diagnostics;
using HusqvarnaAutomowerConnect.App.ViewModels;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Button = Microsoft.UI.Xaml.Controls.Button;
using ScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility;
using ScrollViewer = Microsoft.UI.Xaml.Controls.ScrollViewer;
using StackPanel = Microsoft.UI.Xaml.Controls.StackPanel;
using TextBlock = Microsoft.UI.Xaml.Controls.TextBlock;
using UserControl = Microsoft.UI.Xaml.Controls.UserControl;

namespace HusqvarnaAutomowerConnect.App.Views;

public sealed class MowerDetailsView : UserControl
{
    private readonly MowerDetailsViewModel viewModel;
    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    public MowerDetailsView(MowerDetailsViewModel viewModel)
    {
        AppDiagnostics.Log("MowerDetailsView: constructeur - début.");
        this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        RenderState();
        AppDiagnostics.Log("MowerDetailsView: constructeur - fin.");
    }

    public async Task RefreshAsync(CancellationToken cancellationToken, string? mowerId = null)
    {
        AppDiagnostics.Log($"MowerDetailsView: RefreshAsync - début | {mowerId}");
        await viewModel.LoadAsync(cancellationToken, mowerId);
        RenderState();
        AppDiagnostics.Log("MowerDetailsView: RefreshAsync - fin.");
    }

    private void RenderState()
    {
        if (!dispatcherQueue.HasThreadAccess)
        {
            dispatcherQueue.TryEnqueue(RenderState);
            return;
        }

        AppDiagnostics.Log("MowerDetailsView: RenderState - début.");

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
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            IsTextScaleFactorEnabled = false
        });

        root.Children.Add(CreateText(viewModel.MowerName));
        root.Children.Add(CreateText(viewModel.StateSummary));
        root.Children.Add(CreateText(viewModel.BatterySummary));
        root.Children.Add(CreateText(viewModel.ConnectivitySummary));
        root.Children.Add(CreateText(viewModel.LocationSummary));
        root.Children.Add(CreateText(viewModel.ErrorSummary));
        root.Children.Add(CreateText(viewModel.CommandHint));
        root.Children.Add(CreateText(viewModel.StatusMessage));
        root.Children.Add(new TextBlock { Text = "Durée de tonte temporaire (minutes)" });
        root.Children.Add(CreateText(viewModel.SelectedDurationMinutes.ToString()));

        root.Children.Add(CreateActionButton("Pause", viewModel.CanPause && !viewModel.IsLoading, async () =>
        {
            await viewModel.PauseAsync(CancellationToken.None);
            RenderState();
        }));

        root.Children.Add(CreateActionButton("Reprendre le planning", viewModel.CanResumeSchedule && !viewModel.IsLoading, async () =>
        {
            await viewModel.ResumeScheduleAsync(CancellationToken.None);
            RenderState();
        }));

        root.Children.Add(CreateActionButton("Retour station jusqu'à la prochaine session", viewModel.CanParkUntilNextSchedule && !viewModel.IsLoading, async () =>
        {
            await viewModel.ParkUntilNextScheduleAsync(CancellationToken.None);
            RenderState();
        }));

        root.Children.Add(CreateActionButton("Retour station jusqu'à nouvel ordre", viewModel.CanParkUntilFurtherNotice && !viewModel.IsLoading, async () =>
        {
            await viewModel.ParkUntilFurtherNoticeAsync(CancellationToken.None);
            RenderState();
        }));

        root.Children.Add(CreateActionButton("Lancer la tonte temporaire", viewModel.CanStartForDuration && !viewModel.IsLoading, async () =>
        {
            await viewModel.StartForDurationAsync(CancellationToken.None);
            RenderState();
        }));

        scrollViewer.Content = root;
        Content = scrollViewer;

        AppDiagnostics.Log("MowerDetailsView: RenderState - fin.");
    }

    private static TextBlock CreateText(string text) =>
        new()
        {
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            IsTextScaleFactorEnabled = false
        };

    private static Button CreateActionButton(string label, bool isEnabled, Func<Task> onClick)
    {
        Button button = new()
        {
            Content = label,
            IsEnabled = isEnabled
        };
        button.Click += async (_, _) => await onClick();
        return button;
    }
}
