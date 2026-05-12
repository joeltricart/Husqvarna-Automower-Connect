using HusqvarnaAutomowerConnect.App.ViewModels;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Button = Microsoft.UI.Xaml.Controls.Button;
using StackPanel = Microsoft.UI.Xaml.Controls.StackPanel;
using TextBlock = Microsoft.UI.Xaml.Controls.TextBlock;
using UserControl = Microsoft.UI.Xaml.Controls.UserControl;

namespace HusqvarnaAutomowerConnect.App.Views;

public sealed class LoginView : UserControl
{
    private readonly LoginViewModel viewModel;
    private readonly DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

    public LoginView(LoginViewModel viewModel)
    {
        this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
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
            Text = viewModel.HelperText,
            TextWrapping = TextWrapping.Wrap
        });

        root.Children.Add(new TextBlock { Text = viewModel.ConnectionState, TextWrapping = TextWrapping.Wrap });
        root.Children.Add(new TextBlock { Text = viewModel.SecretState, TextWrapping = TextWrapping.Wrap });
        root.Children.Add(new TextBlock { Text = viewModel.LocalSessionDetail, TextWrapping = TextWrapping.Wrap });

        Button connectButton = new()
        {
            Content = "Se connecter",
            IsEnabled = !viewModel.IsBusy
        };

        Button disconnectButton = new()
        {
            Content = "Supprimer la session locale",
            IsEnabled = !viewModel.IsBusy && viewModel.HasLocalSession
        };
        connectButton.Click += async (_, _) =>
        {
            connectButton.IsEnabled = false;
            disconnectButton.IsEnabled = false;
            await viewModel.ConnectAsync(CancellationToken.None);
            RenderState();
        };
        disconnectButton.Click += async (_, _) =>
        {
            connectButton.IsEnabled = false;
            disconnectButton.IsEnabled = false;
            await viewModel.DisconnectAsync(CancellationToken.None);
            RenderState();
        };

        root.Children.Add(connectButton);
        root.Children.Add(disconnectButton);
        Content = root;
    }
}
