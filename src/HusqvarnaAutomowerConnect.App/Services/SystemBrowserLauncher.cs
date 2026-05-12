using HusqvarnaAutomowerConnect.Core.Errors;
using HusqvarnaAutomowerConnect.Core.Interfaces;
using System.Diagnostics;

namespace HusqvarnaAutomowerConnect.App.Services;

public sealed class SystemBrowserLauncher : IBrowserLauncher
{
    public Task<OperationResult> OpenAsync(Uri uri, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = uri.ToString(),
                UseShellExecute = true
            });

            return Task.FromResult(OperationResult.Success());
        }
        catch (Exception exception)
        {
            return Task.FromResult(OperationResult.Failure(new ApplicationError(
                ApplicationErrorCode.Unknown,
                "Impossible d'ouvrir le navigateur par défaut.",
                exception.Message)));
        }
    }
}
