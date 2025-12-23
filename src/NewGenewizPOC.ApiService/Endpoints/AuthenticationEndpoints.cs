using NewGenewizPOC.ApiService.Models;
using NewGenewizPOC.ApiService.Services;

namespace NewGenewizPOC.ApiService.Endpoints;

/// <summary>
/// Handles all authentication-related endpoints (login, logout, token generation, user info)
/// </summary>
public static class AuthenticationEndpoints
{
    public static void MapAuthenticationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/session")
            .WithTags("Authentication");

        group.MapPost("/login", HandleSessionLogin)
            .WithName("SessionLogin")
            .Produces<SessionLoginResponse>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/logout", HandleSessionLogout)
            .WithName("SessionLogout");

        group.MapGet("/me", HandleGetCurrentUser)
            .WithName("GetCurrentUser")
            .Produces<UserInfoResponse>()
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/token", HandleGetAccessToken)
            .WithName("GetAccessToken")
            .Produces<TokenResponse>()
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleSessionLogin(
        LoginRequest request,
        IAuthenticationService authService,
        ISessionStore sessionStore,
        HttpContext context,
        IConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest(new { error = "Email and password are required" });
        }

        var isValid = await authService.ValidateCredentialsAsync(request.Email, request.Password);
        if (!isValid)
        {
            return Results.Unauthorized();
        }

        var sessionId = sessionStore.CreateSession(request.Email);

        var cookieOptions = BuildCookieOptions(configuration, isDelete: false);
        context.Response.Cookies.Append("sid", sessionId, cookieOptions);

        return Results.Ok(new SessionLoginResponse("Login successful", request.Email));
    }

    private static IResult HandleSessionLogout(
        ISessionStore sessionStore,
        HttpContext context,
        IConfiguration configuration)
    {
        var sessionId = context.Request.Cookies["sid"];
        if (!string.IsNullOrEmpty(sessionId))
        {
            sessionStore.InvalidateSession(sessionId);
        }

        var cookieOptions = BuildCookieOptions(configuration, isDelete: true);
        context.Response.Cookies.Delete("sid", cookieOptions);

        return Results.Ok(new { message = "Logout successful" });
    }

    private static IResult HandleGetCurrentUser(
        ISessionStore sessionStore,
        HttpContext context)
    {
        var sessionId = context.Request.Cookies["sid"];
        if (string.IsNullOrEmpty(sessionId) || !sessionStore.ValidateSession(sessionId))
        {
            return Results.Unauthorized();
        }

        var email = sessionStore.GetEmailFromSession(sessionId);
        if (email == null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new UserInfoResponse(email));
    }

    private static IResult HandleGetAccessToken(
        string? aud,
        ISessionStore sessionStore,
        ITokenService tokenService,
        HttpContext context)
    {
        var sessionId = context.Request.Cookies["sid"];
        if (string.IsNullOrEmpty(sessionId) || !sessionStore.ValidateSession(sessionId))
        {
            return Results.Unauthorized();
        }

        var email = sessionStore.GetEmailFromSession(sessionId);
        if (email == null)
        {
            return Results.Unauthorized();
        }

        var audience = string.IsNullOrWhiteSpace(aud) ? "default" : aud;
        var token = tokenService.GenerateAccessToken(email, audience, expirationMinutes: 15);
        return Results.Ok(new TokenResponse(token, 900)); // 900 seconds = 15 minutes
    }

    private static CookieOptions BuildCookieOptions(IConfiguration configuration, bool isDelete)
    {
        var cookieDomain = configuration["Auth:CookieDomain"];
        var sameSiteSetting = configuration["Auth:SameSite"] ?? configuration["Auth:CookieSameSite"];
        var secureSetting = configuration["Auth:CookieSecure"];

        var sameSite = SameSiteMode.Lax;
        if (!string.IsNullOrWhiteSpace(sameSiteSetting) && Enum.TryParse<SameSiteMode>(sameSiteSetting, true, out var parsedSameSite))
        {
            sameSite = parsedSameSite;
        }

        var secure = false;
        if (!string.IsNullOrWhiteSpace(secureSetting) && bool.TryParse(secureSetting, out var parsedSecure))
        {
            secure = parsedSecure;
        }

        var expiresMinutes = configuration.GetValue<int?>("Auth:SessionMinutes") ?? 30;
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = sameSite,
            Path = "/",
            Expires = isDelete ? DateTimeOffset.UtcNow.AddDays(-1) : DateTimeOffset.UtcNow.AddMinutes(expiresMinutes),
            Domain = string.IsNullOrWhiteSpace(cookieDomain) ? null : cookieDomain
        };
    }
}
