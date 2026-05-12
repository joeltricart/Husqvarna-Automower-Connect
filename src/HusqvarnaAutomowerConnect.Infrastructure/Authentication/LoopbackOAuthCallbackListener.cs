using System.Net;
using System.Text;
using HusqvarnaAutomowerConnect.Core.Errors;
using HusqvarnaAutomowerConnect.Core.Interfaces;
using HusqvarnaAutomowerConnect.Core.Models;

namespace HusqvarnaAutomowerConnect.Infrastructure.Authentication;

public sealed class LoopbackOAuthCallbackListener : IOAuthCallbackListener
{
    public async Task<OperationResult<OAuthCallbackResult>> WaitForCallbackAsync(
        Uri redirectUri,
        string expectedState,
        CancellationToken cancellationToken)
    {
        if (redirectUri is null)
        {
            return OperationResult<OAuthCallbackResult>.Failure(new ApplicationError(
                ApplicationErrorCode.Validation,
                "Une URI de redirection est requise."));
        }

        string prefix = BuildPrefix(redirectUri);
        HttpListener listener = new();
        listener.Prefixes.Add(prefix);

        try
        {
            listener.Start();
        }
        catch (Exception exception)
        {
            return OperationResult<OAuthCallbackResult>.Failure(new ApplicationError(
                ApplicationErrorCode.SecureStorageUnavailable,
                "Le retour local OAuth ne peut pas être démarré sur cette machine.",
                exception.Message));
        }

        try
        {
            using CancellationTokenRegistration registration = cancellationToken.Register(() => SafeStop(listener));
            HttpListenerContext context = await listener.GetContextAsync();
            OAuthCallbackResult callback = ParseCallback(context.Request.Url);
            if (!string.IsNullOrWhiteSpace(expectedState) && !string.Equals(callback.State, expectedState, StringComparison.Ordinal))
            {
                callback = callback with
                {
                    Error = "invalid_state",
                    ErrorDescription = "L'état OAuth reçu ne correspond pas à la requête initiale."
                };
            }
            await WriteResponseAsync(context.Response, callback, cancellationToken);
            return OperationResult<OAuthCallbackResult>.Success(callback);
        }
        catch (OperationCanceledException)
        {
            return OperationResult<OAuthCallbackResult>.Failure(new ApplicationError(
                ApplicationErrorCode.Validation,
                "La connexion a été annulée."));
        }
        catch (Exception exception)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return OperationResult<OAuthCallbackResult>.Failure(new ApplicationError(
                    ApplicationErrorCode.Validation,
                    "La connexion a été annulée."));
            }

            return OperationResult<OAuthCallbackResult>.Failure(new ApplicationError(
                ApplicationErrorCode.Unknown,
                "La réponse de rappel OAuth est invalide ou inaccessible.",
                exception.Message));
        }
        finally
        {
            SafeStop(listener);
            listener.Close();
        }
    }

    private static string BuildPrefix(Uri redirectUri)
    {
        UriBuilder builder = new(redirectUri)
        {
            Path = string.IsNullOrWhiteSpace(redirectUri.AbsolutePath) || redirectUri.AbsolutePath == "/"
                ? "/"
                : redirectUri.AbsolutePath.TrimEnd('/') + "/"
        };

        return builder.Uri.AbsoluteUri;
    }

    private static OAuthCallbackResult ParseCallback(Uri? url)
    {
        if (url is null)
        {
            return new OAuthCallbackResult
            {
                Error = "invalid_callback",
                ErrorDescription = "L'URI de rappel est absente."
            };
        }

        Dictionary<string, string?> query = ParseQuery(url.Query);
        return new OAuthCallbackResult
        {
            Code = query.TryGetValue("code", out string? code) ? code : null,
            State = query.TryGetValue("state", out string? state) ? state : null,
            Error = query.TryGetValue("error", out string? error) ? error : null,
            ErrorDescription = query.TryGetValue("error_description", out string? description) ? description : null
        };
    }

    private static Dictionary<string, string?> ParseQuery(string query)
    {
        Dictionary<string, string?> result = [];
        if (string.IsNullOrWhiteSpace(query))
        {
            return result;
        }

        foreach (string pair in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string[] parts = pair.Split('=', 2);
            string key = Uri.UnescapeDataString(parts[0]);
            string? value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : string.Empty;
            result[key] = value;
        }

        return result;
    }

    private static async Task WriteResponseAsync(
        HttpListenerResponse response,
        OAuthCallbackResult callback,
        CancellationToken cancellationToken)
    {
        response.StatusCode = 200;
        response.ContentType = "text/html; charset=utf-8";

        string message = string.IsNullOrWhiteSpace(callback.Error)
            ? "Connexion terminée. Vous pouvez revenir dans l'application."
            : "La connexion OAuth a échoué. Vous pouvez fermer cette fenêtre.";

        string html = $"""
            <!doctype html>
            <html lang="fr">
            <head>
              <meta charset="utf-8" />
              <title>Husqvarna Automower Connect</title>
            </head>
            <body>
              <h1>{WebUtility.HtmlEncode(message)}</h1>
              <p>Vous pouvez fermer cet onglet.</p>
            </body>
            </html>
            """;

        byte[] buffer = Encoding.UTF8.GetBytes(html);
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer, cancellationToken);
        response.OutputStream.Close();
    }

    private static void SafeStop(HttpListener listener)
    {
        try
        {
            if (listener.IsListening)
            {
                listener.Stop();
            }
        }
        catch
        {
        }
    }
}
