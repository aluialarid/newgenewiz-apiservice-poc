using System.Text;
using NewGenewizPOC.ApiService.Models;
using System.Text.Json;

namespace NewGenewizPOC.ApiService.Services;

/// <summary>
/// Handles communication with backend microservices
/// </summary>
public interface IMicroserviceClient
{
    Task<List<OrderDto>> GetOrdersAsync();
    Task<List<EnrichedOrderDto>> GetEnrichedOrdersAsync();
}

public class MicroserviceClient : IMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MicroserviceClient> _logger;

    public MicroserviceClient(HttpClient httpClient, ILogger<MicroserviceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<OrderDto>> GetOrdersAsync()
    {
        try
        {
            _logger.LogInformation("Calling Order Service to get orders");
            var response = await _httpClient.GetAsync("/orders");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var orders = JsonSerializer.Deserialize<List<OrderDto>>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                _logger.LogInformation("Successfully retrieved {Count} orders", orders?.Count ?? 0);
                return orders ?? new List<OrderDto>();
            }

            _logger.LogError("Order Service returned {StatusCode}", response.StatusCode);
            return new List<OrderDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Order Service");
            throw;
        }
    }

    public async Task<List<EnrichedOrderDto>> GetEnrichedOrdersAsync()
    {
        try
        {
            _logger.LogInformation("Calling Order Service to get enriched orders");

            // POST requires an HttpContent object (even if empty)
            using var content = new StringContent("{}", Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/orders/enrich", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var orders = JsonSerializer.Deserialize<List<EnrichedOrderDto>>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                _logger.LogInformation("Successfully retrieved {Count} enriched orders", orders?.Count ?? 0);
                return orders ?? new List<EnrichedOrderDto>();
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Order Service returned {StatusCode}: {Content}", response.StatusCode, errorContent);
            return new List<EnrichedOrderDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Order Service for enrichment");
            throw;
        }
    }
}

