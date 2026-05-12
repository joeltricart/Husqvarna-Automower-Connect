using HusqvarnaAutomowerConnect.Core.Errors;
using HusqvarnaAutomowerConnect.Core.Models;

namespace HusqvarnaAutomowerConnect.Core.Interfaces;

public interface IOAuthCallbackListener
{
    Task<OperationResult<OAuthCallbackResult>> WaitForCallbackAsync(
        Uri redirectUri,
        string expectedState,
        CancellationToken cancellationToken);
}
