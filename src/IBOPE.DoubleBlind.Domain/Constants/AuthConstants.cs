namespace IBOPE.DoubleBlind.Domain.Constants;

public static class AuthConstants
{
    public const string CookieScheme = "DoubleBlind.Cookie";
    public const string LoginPath = "/login";
    public const string HomePath = "/home";
    public const int MaxLoginAttemptsPerMinute = 5;
}
