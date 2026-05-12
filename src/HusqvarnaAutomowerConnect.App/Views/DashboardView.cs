using HusqvarnaAutomowerConnect.App.ViewModels;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Border = Microsoft.UI.Xaml.Controls.Border;
using Button = Microsoft.UI.Xaml.Controls.Button;
using ScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility;
using ScrollViewer = Microsoft.UI.Xaml.Controls.ScrollViewer;
using StackPanel = Microsoft.UI.Xaml.Controls.StackPanel;
using TextBlock = Microsoft.UI.Xaml.Controls.TextBlock;
using Thickness = Microsoft.UI.Xaml.Thickness;
using UserControl = Microsoft.UI.Xaml.Controls.UserControl;

namespace HusqvarnaAutomowerConnect.App.Views;

public sealed class DashboardView : UserControl
{
    private readonly DashboardViewModel viewModel;
    private readonly Func<string, Task> openDetailsAsync;
    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    public DashboardView(DashboardViewModel viewModel, Func<string, Task> openDetailsAsync)
    {
        this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        this.openDetailsAsync = openDetailsAsync ?? throw new ArgumentNullException(nameof(openDetailsAsync));
        RenderState();
    }

    public async Task RefreshAsync(CancellationToken cancellationToken)
    {
        await viewModel.RefreshAsync(cancellationToken);
        RenderState();
    }

    private void RenderState()
    {
        if (!dispatcherQueue.HasThreadAccess)
        {
            dispatcherQueue.TryEnqueue(RenderState);
            return;
        }

        ScrollViewer scrollViewer = new()
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        StackPanel root = new()
        {
            Padding = new Thickness(24),
            Spacing = 16
        };

        root.Children.Add(new TextBlock
        {
            Text = viewModel.Title,
            FontSize = 24,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
        });

        root.Children.Add(new TextBlock
        {
            Text = viewModel.StatusMessage,
            TextWrapping = TextWrapping.Wrap
        });

        root.Children.Add(new TextBlock
        {
            Text = viewModel.RefreshInfo,
            TextWrapping = TextWrapping.Wrap
        });

        Button refreshButton = new()
        {
            Content = viewModel.IsLoading ? "Chargement..." : "Actualiser la liste",
            IsEnabled = !viewModel.IsLoading
        };
        refreshButton.Click += async (_, _) =>
        {
            await RefreshAsync(CancellationToken.None);
        };
        root.Children.Add(refreshButton);

        if (!string.IsNullOrWhiteSpace(viewModel.ErrorMessage))
        {
            root.Children.Add(new TextBlock
            {
                Text = viewModel.ErrorMessage,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.IndianRed)
            });
        }

        if (viewModel.Cards.Count == 0)
        {
            root.Children.Add(new TextBlock
            {
                Text = viewModel.EmptyStateMessage,
                TextWrapping = TextWrapping.Wrap
            });
        }
        else
        {
            foreach (MowerDashboardCard card in viewModel.Cards)
            {
                root.Children.Add(CreateCard(card));
            }
        }

        scrollViewer.Content = root;
        Content = scrollViewer;
    }

    private UIElement CreateCard(MowerDashboardCard card)
    {
        Border border = new()
        {
            Padding = new Thickness(16),
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.LightGray),
            Margin = new Thickness(0, 0, 0, 12)
        };

        StackPanel panel = new()
        {
            Spacing = 6
        };

        panel.Children.Add(new TextBlock
        {
            Text = card.Name,
            FontSize = 18,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
        });

        panel.Children.Add(new TextBlock { Text = card.Details, TextWrapping = TextWrapping.Wrap });
        panel.Children.Add(new TextBlock { Text = card.StatusLine, TextWrapping = TextWrapping.Wrap });
        panel.Children.Add(new TextBlock { Text = card.BatteryLine, TextWrapping = TextWrapping.Wrap });
        panel.Children.Add(new TextBlock { Text = card.ConnectivityLine, TextWrapping = TextWrapping.Wrap });
        panel.Children.Add(new TextBlock { Text = card.LocationLine, TextWrapping = TextWrapping.Wrap });
        panel.Children.Add(new TextBlock { Text = card.UpdatedLine, TextWrapping = TextWrapping.Wrap });
        panel.Children.Add(new TextBlock { Text = card.ErrorLine, TextWrapping = TextWrapping.Wrap });

        Button detailsButton = new()
        {
            Content = "Voir le détail",
            Margin = new Thickness(0, 8, 0, 0)
        };
        detailsButton.Click += async (_, _) => await openDetailsAsync(card.Id);
        panel.Children.Add(detailsButton);

        border.Child = panel;
        return border;
    }
}
