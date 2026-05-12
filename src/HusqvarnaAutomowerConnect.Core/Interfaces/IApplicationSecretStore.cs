using HusqvarnaAutomowerConnect.Core.Errors;

namespace HusqvarnaAutomowerConnect.Core.Interfaces;

public interface IApplicationSecretStore
{
    Task<OperationResult<string?>> LoadAsync(CancellationToken cancellationToken);

    Task<OperationResult> SaveAsync(string applicationSecret, CancellationToken cancellationToken);

    Task<OperationResult> DeleteAsync(CancellationToken cancellationToken);
}
