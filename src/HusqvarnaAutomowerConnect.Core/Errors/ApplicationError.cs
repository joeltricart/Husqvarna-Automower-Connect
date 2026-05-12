namespace HusqvarnaAutomowerConnect.Core.Errors;

public sealed record ApplicationError(
    ApplicationErrorCode Code,
    string UserMessage,
    string? TechnicalMessage = null,
    int? HttpStatusCode = null);

