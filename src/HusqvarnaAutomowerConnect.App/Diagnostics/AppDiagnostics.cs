using System.Diagnostics;
using System.Text;
using Timer = System.Threading.Timer;

namespace HusqvarnaAutomowerConnect.App.Diagnostics;

public static class AppDiagnostics
{
    private static readonly object SyncRoot = new();
    private static readonly string LogDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "HusqvarnaAutomowerConnect",
        "logs");
    private static readonly string LogFilePath = Path.Combine(LogDirectory, "startup.log");
    private static Timer? heartbeatTimer;

    public static string LogFilePathValue => LogFilePath;

    public static void Initialize()
    {
        Directory.CreateDirectory(LogDirectory);
        Log("Diagnostics initialisés.");
    }

    public static void Log(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        string line = $"{DateTimeOffset.Now:O} | {message}";
        lock (SyncRoot)
        {
            File.AppendAllText(LogFilePath, line + Environment.NewLine, Encoding.UTF8);
        }
    }

    public static void LogException(string context, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        string detail = exception.GetType().FullName ?? exception.GetType().Name;
        Log($"{context} | {detail} | {exception.Message}");
        if (exception.StackTrace is { Length: > 0 })
        {
            Log(exception.StackTrace);
        }
    }

    public static void AttachGlobalHandlers()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception exception)
            {
                LogException("UnhandledException", exception);
            }
            else
            {
                Log($"UnhandledException | Objet non exception: {args.ExceptionObject}");
            }
        };

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            LogException("UnobservedTaskException", args.Exception);
            args.SetObserved();
        };
    }

    public static void StartHeartbeat(string scope)
    {
        StopHeartbeat();
        heartbeatTimer = new Timer(
            _ => Log($"Heartbeat | {scope} | {Process.GetCurrentProcess().Id}"),
            null,
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(5));
    }

    public static void StopHeartbeat()
    {
        heartbeatTimer?.Dispose();
        heartbeatTimer = null;
    }
}
