using HusqvarnaAutomowerConnect.Core.Errors;
using HusqvarnaAutomowerConnect.Core.Models;

namespace HusqvarnaAutomowerConnect.Core.Interfaces;

public interface ISecureTokenStore
{
    Task<OperationResult> SaveAsync(AuthSession session, CancellationToken cancellationToken);

    Task<OperationResult<AuthSession?>> LoadAsync(CancellationToken cancellationToken);

    Task<OperationResult> DeleteAsync(CancellationToken cancellationToken);
}

