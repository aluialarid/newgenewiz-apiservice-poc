using NewGenewizPOC.ApiService.Services;

namespace NewGenewizPOC.ApiService.Extensions;

/// <summary>
/// Extension methods for configuring HTTP clients used to call backend microservices.
/// </summary>
public static class MicroserviceClientExtensions
{
    /// <summary>
    /// Registers <see cref="IMicroserviceClient"/> as a typed <see cref="HttpClient"/> client.
    /// 
    /// IMPORTANT: In Aspire, service discovery is enabled via ServiceDefaults (ConfigureHttpClientDefaults).
    /// Setting BaseAddress to "http://orderservice" lets the discovery handler resolve the service.
    /// </summary>
    public static IServiceCollection AddMicroserviceClient(this IServiceCollection services)
    {
        services.AddHttpClient<IMicroserviceClient, MicroserviceClient>(client =>
        {
            // Service name must match the AppHost resource name: builder.AddProject(..., "orderservice")
            // Scheme is typically http inside the dev orchestrator.
            client.BaseAddress = new Uri("http://orderservice");
        });

        // Note: no .AddServiceDiscovery() here because builder.AddServiceDefaults()
        // already configures HttpClient defaults to include service discovery.

        return services;
    }
}

