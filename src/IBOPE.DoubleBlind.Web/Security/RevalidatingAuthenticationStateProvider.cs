using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace IBOPE.DoubleBlind.Web.Security;

public sealed class RevalidatingAuthenticationStateProvider(ILoggerFactory loggerFactory)
    : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

    protected override Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState,
        CancellationToken cancellationToken)
    {
        var user = authenticationState.User;
        var isValid = user.Identity?.IsAuthenticated == true
            && !string.IsNullOrWhiteSpace(user.FindFirstValue(ClaimTypes.NameIdentifier));

        return Task.FromResult(isValid);
    }
}
