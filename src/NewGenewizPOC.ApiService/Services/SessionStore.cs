namespace NewGenewizPOC.ApiService.Services;

/// <summary>
/// In-memory session store for the POC.
/// In production, use a distributed cache (Redis) or database.
/// Note: this store is per-process and will not survive restarts or scale-out.
/// </summary>
public interface ISessionStore
{
    string CreateSession(string email);
    bool ValidateSession(string sessionId);
    string? GetEmailFromSession(string sessionId);
    void InvalidateSession(string sessionId);
}

public class InMemorySessionStore : ISessionStore
{
    private readonly Dictionary<string, SessionData> _sessions = new();
    private readonly object _lock = new();
    private readonly ILogger<InMemorySessionStore> _logger;
    private const int SessionExpirationMinutes = 30;

    public InMemorySessionStore(ILogger<InMemorySessionStore> logger)
    {
        _logger = logger;
    }

    public string CreateSession(string email)
    {
        lock (_lock)
        {
            var sessionId = Guid.NewGuid().ToString();
            _sessions[sessionId] = new SessionData
            {
                Email = email,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(SessionExpirationMinutes)
            };

            _logger.LogInformation("Session created: {SessionId} for {Email}", sessionId, email);
            return sessionId;
        }
    }

    public bool ValidateSession(string sessionId)
    {
        lock (_lock)
        {
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                // Clean up expired sessions
                if (DateTime.UtcNow > session.ExpiresAt)
                {
                    _sessions.Remove(sessionId);
                    _logger.LogInformation("Session expired: {SessionId}", sessionId);
                    return false;
                }

                // Extend session on each validation
                session.ExpiresAt = DateTime.UtcNow.AddMinutes(SessionExpirationMinutes);
                return true;
            }

            return false;
        }
    }

    public string? GetEmailFromSession(string sessionId)
    {
        lock (_lock)
        {
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                if (DateTime.UtcNow <= session.ExpiresAt)
                {
                    return session.Email;
                }

                _sessions.Remove(sessionId);
            }

            return null;
        }
    }

    public void InvalidateSession(string sessionId)
    {
        lock (_lock)
        {
            if (_sessions.Remove(sessionId))
            {
                _logger.LogInformation("Session invalidated: {SessionId}", sessionId);
            }
        }
    }

    private class SessionData
    {
        public string Email { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
