using System.Security.Claims;
using IBOPE.DoubleBlind.Application.DTOs;
using IBOPE.DoubleBlind.Application.Interfaces;
using IBOPE.DoubleBlind.Domain.Constants;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IBOPE.DoubleBlind.Web.Security;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/account")
            .RequireRateLimiting("login");

        group.MapPost("/login", (Delegate)LoginAsync).AllowAnonymous();
        group.MapPost("/logout", (Delegate)LogoutAsync).RequireAuthorization();

        return endpoints;
    }

    private static async Task<IResult> LoginAsync(
        HttpContext httpContext,
        IAntiforgery antiforgery,
        [FromForm] LoginRequestDto request,
        ILoginService loginService)
    {
        await antiforgery.ValidateRequestAsync(httpContext);

        var result = await loginService.AuthenticateAsync(request);

        if (!result.Succeeded || result.UserId is null)
        {
            return Results.Redirect($"{AuthConstants.LoginPath}?error=invalid");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.UserId.Value.ToString()),
            new(ClaimTypes.Name, result.DisplayName ?? request.Email),
            new(ClaimTypes.Email, request.Email)
        };

        var identity = new ClaimsIdentity(claims, AuthConstants.CookieScheme);
        var principal = new ClaimsPrincipal(identity);

        await httpContext.SignInAsync(
            AuthConstants.CookieScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = request.RememberMe,
                AllowRefresh = true
            });

        return Results.Redirect(AuthConstants.HomePath);
    }

    private static async Task<IResult> LogoutAsync(
        HttpContext httpContext,
        IAntiforgery antiforgery)
    {
        await antiforgery.ValidateRequestAsync(httpContext);
        await httpContext.SignOutAsync(AuthConstants.CookieScheme);
        return Results.Redirect(AuthConstants.LoginPath);
    }
}
