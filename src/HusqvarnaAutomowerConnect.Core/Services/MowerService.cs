using HusqvarnaAutomowerConnect.Core.Errors;
using HusqvarnaAutomowerConnect.Core.Interfaces;
using HusqvarnaAutomowerConnect.Core.Models;

namespace HusqvarnaAutomowerConnect.Core.Services;

public sealed class MowerService(
    IHusqvarnaApiClient apiClient,
    CommandAvailabilityService commandAvailabilityService) : IMowerService
{
    public Task<OperationResult<IReadOnlyList<Mower>>> GetMowersAsync(CancellationToken cancellationToken) =>
        apiClient.GetMowersAsync(cancellationToken);

    public Task<OperationResult<Mower>> GetMowerAsync(string mowerId, CancellationToken cancellationToken) =>
        apiClient.GetMowerAsync(mowerId, cancellationToken);

    public async Task<OperationResult<CommandResult>> SendCommandAsync(
        string mowerId,
        MowerCommand command,
        CancellationToken cancellationToken)
    {
        OperationResult<Mower> mowerResult = await apiClient.GetMowerAsync(mowerId, cancellationToken);
        if (!mowerResult.IsSuccess)
        {
            return OperationResult<CommandResult>.Failure(
                mowerResult.Error ?? new ApplicationError(
                    ApplicationErrorCode.Unknown,
                    "Impossible de charger le robot avant l'envoi de la commande."));
        }

        OperationResult availability = commandAvailabilityService.EnsureAvailable(mowerResult.Value, command);
        if (!availability.IsSuccess)
        {
            return OperationResult<CommandResult>.Failure(
                availability.Error ?? new ApplicationError(
                    ApplicationErrorCode.UnsupportedCommand,
                    "Cette commande n'est pas disponible."));
        }

        return await apiClient.SendCommandAsync(mowerId, command, cancellationToken);
    }
}
