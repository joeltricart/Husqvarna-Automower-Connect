using HusqvarnaAutomowerConnect.Core.Errors;

namespace HusqvarnaAutomowerConnect.Core.Interfaces;

public interface IBrowserLauncher
{
    Task<OperationResult> OpenAsync(Uri uri, CancellationToken cancellationToken);
}
