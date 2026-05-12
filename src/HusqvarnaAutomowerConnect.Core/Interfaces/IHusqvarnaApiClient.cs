using HusqvarnaAutomowerConnect.Core.Errors;
using HusqvarnaAutomowerConnect.Core.Models;

namespace HusqvarnaAutomowerConnect.Core.Interfaces;

public interface IHusqvarnaApiClient
{
    Task<OperationResult<IReadOnlyList<Mower>>> GetMowersAsync(CancellationToken cancellationToken);

    Task<OperationResult<Mower>> GetMowerAsync(string mowerId, CancellationToken cancellationToken);

    Task<OperationResult<CommandResult>> SendCommandAsync(
        string mowerId,
        MowerCommand command,
        CancellationToken cancellationToken);
}

