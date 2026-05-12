using HusqvarnaAutomowerConnect.App.Composition;
using HusqvarnaAutomowerConnect.App.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Application = Microsoft.UI.Xaml.Application;
using Microsoft.UI.Xaml;

namespace HusqvarnaAutomowerConnect.App;

public sealed class App : Application
{
    private readonly ServiceProvider serviceProvider;
    private Window? window;

    public App()
    {
        AppDiagnostics.Log("Constructeur App.");
        AppDiagnostics.AttachGlobalHandlers();
        UnhandledException += App_UnhandledException;
        ServiceCollection services = new();
        services.AddHusqvarnaAutomowerConnect();
        serviceProvider = services.BuildServiceProvider();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        AppDiagnostics.Log("OnLaunched.");
        window = new MainWindow(serviceProvider);
        window.Activate();
    }

    private static void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        if (e.Exception is not null)
        {
            AppDiagnostics.LogException("WinUI UnhandledException", e.Exception);
        }
        else
        {
            AppDiagnostics.Log($"WinUI UnhandledException | {e.Message}");
        }
    }
}
