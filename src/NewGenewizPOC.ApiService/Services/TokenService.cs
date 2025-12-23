using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace NewGenewizPOC.ApiService.Services;

/// <summary>
/// Handles JWT token generation and validation
/// </summary>
public interface ITokenService
{
    string GenerateAccessToken(string email, string? audience = null, int expirationMinutes = 15);
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenService> _logger;

    public TokenService(IConfiguration configuration, ILogger<TokenService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }


    public string GenerateAccessToken(string email, string? audience = null, int expirationMinutes = 15)
    {
        var key = Encoding.ASCII.GetBytes(GetSecretKey());
        var issuer = _configuration["JwtSettings:Issuer"];
        var defaultAudience = _configuration["JwtSettings:DefaultAudience"];

        var resolvedAudience = !string.IsNullOrWhiteSpace(audience) ? audience : defaultAudience;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, email),
            new Claim(ClaimTypes.Email, email)
        };

        if (!string.IsNullOrWhiteSpace(resolvedAudience))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Aud, resolvedAudience));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Issuer = issuer,
            Audience = resolvedAudience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        _logger.LogInformation("Access token generated for user: {Email}, audience: {Audience}", email, resolvedAudience ?? "default");
        return tokenHandler.WriteToken(token);
    }

    private string GetSecretKey() =>
        _configuration["JwtSettings:SecretKey"] ?? "ThisIsASecretKeyForTheAzentaPOCProject2025!";
}
