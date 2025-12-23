namespace NewGenewizPOC.ApiService.Services;

/// <summary>
/// Handles authentication logic
/// </summary>
public interface IAuthenticationService
{
    Task<bool> ValidateCredentialsAsync(string email, string password);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(ILogger<AuthenticationService> logger)
    {
        _logger = logger;
    }

    public Task<bool> ValidateCredentialsAsync(string email, string password)
    {
        // For POC: Simple hardcoded credentials
        // In production: Query a database or external auth service
        var isValid = email == "admin@azenta.com" && password == "password";

        if (isValid)
        {
            _logger.LogInformation("Authentication successful for: {Email}", email);
        }
        else
        {
            _logger.LogWarning("Authentication failed for: {Email}", email);
        }

        return Task.FromResult(isValid);
    }
}

