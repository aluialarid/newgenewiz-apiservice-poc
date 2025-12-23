using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NewGenewizPOC.ApiService.Services;

namespace NewGenewizPOC.ApiService.Extensions;

/// <summary>
/// Extension methods for setting up authentication and authorization services
/// </summary>
public static class AuthenticationExtensions
{
    private const string DefaultPocSecretKey = "ThisIsASecretKeyForTheAzentaPOCProject2025!";

    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var secretKey = configuration["JwtSettings:SecretKey"] ?? DefaultPocSecretKey;
        var key = Encoding.ASCII.GetBytes(secretKey);
        var issuer = configuration["JwtSettings:Issuer"];
        var defaultAudience = configuration["JwtSettings:DefaultAudience"];
        var validAudiences = configuration.GetSection("JwtSettings:ValidAudiences").Get<string[]>() ?? Array.Empty<string>();

        if (!string.IsNullOrWhiteSpace(defaultAudience) && validAudiences.Length == 0)
        {
            validAudiences = [defaultAudience];
        }

        // Configure JWT Bearer authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = !string.IsNullOrWhiteSpace(issuer),
                ValidIssuer = issuer,
                ValidateAudience = validAudiences.Length > 0,
                ValidAudiences = validAudiences
            };
        });

        // Add authorization
        services.AddAuthorization();

        // Register application services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<ISessionStore, InMemorySessionStore>();

        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Auth:AllowedOrigins").Get<string[]>()
            ?.Where(o => !string.IsNullOrWhiteSpace(o))
            .ToArray() ?? [];

        if (allowedOrigins.Length == 0)
        {
            throw new InvalidOperationException(
                "Auth:AllowedOrigins must be configured. In Aspire, this is injected via AppHost.cs");
        }

        services.AddCors(options =>
        {
            options.AddPolicy("AuthCors", policy =>
            {
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        return services;
    }

    public static WebApplication UseAuthenticationMiddleware(this WebApplication app)
    {
        app.UseCors("AuthCors");
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
