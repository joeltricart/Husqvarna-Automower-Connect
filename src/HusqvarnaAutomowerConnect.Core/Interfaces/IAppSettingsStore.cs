using HusqvarnaAutomowerConnect.Core.Errors;
using HusqvarnaAutomowerConnect.Core.Models;

namespace HusqvarnaAutomowerConnect.Core.Interfaces;

public interface IAppSettingsStore
{
    Task<OperationResult<AppSettings>> LoadAsync(CancellationToken cancellationToken);

    Task<OperationResult> SaveAsync(AppSettings settings, CancellationToken cancellationToken);
}

