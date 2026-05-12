using HusqvarnaAutomowerConnect.Core.Errors;

namespace HusqvarnaAutomowerConnect.Infrastructure.Api;

public static class ApiErrorMapper
{
    public static ApplicationError FromStatusCode(int statusCode, string? technicalMessage = null) =>
        statusCode switch
        {
            400 => new ApplicationError(
                ApplicationErrorCode.Validation,
                "La requête envoyée à Husqvarna est invalide.",
                technicalMessage,
                statusCode),
            401 => new ApplicationError(
                ApplicationErrorCode.Unauthorized,
                "Session expirée. Veuillez vous reconnecter.",
                technicalMessage,
                statusCode),
            403 => new ApplicationError(
                ApplicationErrorCode.Forbidden,
                "Accès refusé. Vérifiez que l'application Husqvarna Developer est bien connectée aux API nécessaires.",
                technicalMessage,
                statusCode),
            404 => new ApplicationError(
                ApplicationErrorCode.NotFound,
                "La ressource demandée est introuvable.",
                technicalMessage,
                statusCode),
            415 => new ApplicationError(
                ApplicationErrorCode.Validation,
                "Le format de requête n'est pas supporté par Husqvarna.",
                technicalMessage,
                statusCode),
            429 => new ApplicationError(
                ApplicationErrorCode.RateLimited,
                "Trop de requêtes envoyées à Husqvarna. Le rafraîchissement est temporairement ralenti.",
                technicalMessage,
                statusCode),
            500 => new ApplicationError(
                ApplicationErrorCode.ServiceUnavailable,
                "Service Husqvarna temporairement indisponible. Réessayez plus tard.",
                technicalMessage,
                statusCode),
            503 => new ApplicationError(
                ApplicationErrorCode.ServiceUnavailable,
                "Service Husqvarna temporairement indisponible. Réessayez plus tard.",
                technicalMessage,
                statusCode),
            _ => new ApplicationError(
                ApplicationErrorCode.Unknown,
                "Une erreur inconnue est survenue lors de l'appel Husqvarna.",
                technicalMessage,
                statusCode)
        };
}

