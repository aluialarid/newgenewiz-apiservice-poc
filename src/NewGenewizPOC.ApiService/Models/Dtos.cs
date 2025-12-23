namespace NewGenewizPOC.ApiService.Models;

/// <summary>
/// Request model for login
/// </summary>
public record LoginRequest(string Email, string Password);

/// <summary>
/// Response model for session login (no token in body, session via cookie)
/// </summary>
public record SessionLoginResponse(string Message, string Email);

/// <summary>
/// Response model for user info
/// </summary>
public record UserInfoResponse(string Email, string Message = "OK");

/// <summary>
/// Response model for token endpoint
/// </summary>
public record TokenResponse(string AccessToken, int ExpiresIn);

/// <summary>
/// Data Transfer Object for Order
/// </summary>
public record OrderDto(
    int Id,
    string OrderNumber,
    string CustomerName,
    string OrderDate,
    decimal TotalAmount
);

/// <summary>
/// Data Transfer Object for Enriched Order (includes pricing)
/// </summary>
public record EnrichedOrderDto(
    int Id,
    string OrderNumber,
    string CustomerName,
    string OrderDate,
    decimal TotalAmount,
    decimal UnitPrice
);

