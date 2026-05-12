using Microsoft.UI.Xaml;
using Application = Microsoft.UI.Xaml.Application;
using HusqvarnaAutomowerConnect.App.Diagnostics;

namespace HusqvarnaAutomowerConnect.App;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        AppDiagnostics.Initialize();
        AppDiagnostics.Log("Programme principal démarré.");
        Application.Start(_ =>
        {
            AppDiagnostics.Log("Application.Start a créé App.");
            new App();
        });
    }
}
