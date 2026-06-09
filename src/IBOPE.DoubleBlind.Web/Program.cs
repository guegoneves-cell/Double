using IBOPE.DoubleBlind.Domain.Constants;
using IBOPE.DoubleBlind.Infrastructure.DependencyInjection;
using IBOPE.DoubleBlind.Web.Components;
using IBOPE.DoubleBlind.Web.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(AuthConstants.CookieScheme)
    .AddCookie(AuthConstants.CookieScheme, options =>
    {
        options.Cookie.Name = "DoubleBlind.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.LoginPath = AuthConstants.LoginPath;
        options.LogoutPath = "/account/logout";
        options.AccessDeniedPath = AuthConstants.LoginPath;
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingAuthenticationStateProvider>();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", limiter =>
    {
        limiter.PermitLimit = AuthConstants.MaxLoginAttemptsPerMinute;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();

app.MapGet("/", (HttpContext context) =>
    context.User.Identity?.IsAuthenticated == true
        ? Results.Redirect(AuthConstants.HomePath)
        : Results.Redirect(AuthConstants.LoginPath))
    .AllowAnonymous();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
