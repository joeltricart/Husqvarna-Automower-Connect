using HusqvarnaAutomowerConnect.Core.Errors;
using HusqvarnaAutomowerConnect.Core.Models;

namespace HusqvarnaAutomowerConnect.Core.Services;

public sealed class CommandAvailabilityService
{
    public OperationResult EnsureAvailable(Mower? mower, MowerCommand command)
    {
        if (mower is null)
        {
            return OperationResult.Failure(new ApplicationError(
                ApplicationErrorCode.NoMowerFound,
                "Aucun robot correspondant n'a été trouvé."));
        }

        bool isSupported = command.Type switch
        {
            MowerCommandType.Pause => mower.Capabilities.CanPause,
            MowerCommandType.ParkUntilNextSchedule => mower.Capabilities.CanParkUntilNextSchedule,
            MowerCommandType.ParkUntilFurtherNotice => mower.Capabilities.CanParkUntilFurtherNotice,
            MowerCommandType.ResumeSchedule => mower.Capabilities.CanResumeSchedule,
            MowerCommandType.StartForDuration => mower.Capabilities.CanStartForDuration,
            _ => false
        };

        if (!isSupported)
        {
            return OperationResult.Failure(new ApplicationError(
                ApplicationErrorCode.UnsupportedCommand,
                "Cette commande n'est pas disponible pour ce robot ou dans son état actuel."));
        }

        if (command.Type == MowerCommandType.StartForDuration &&
            (!command.Duration.HasValue || command.Duration.Value <= TimeSpan.Zero))
        {
            return OperationResult.Failure(new ApplicationError(
                ApplicationErrorCode.Validation,
                "La durée de tonte temporaire doit être strictement positive."));
        }

        return OperationResult.Success();
    }
}

