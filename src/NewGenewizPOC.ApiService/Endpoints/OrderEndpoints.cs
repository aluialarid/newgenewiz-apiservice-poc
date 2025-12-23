using NewGenewizPOC.ApiService.Services;

namespace NewGenewizPOC.ApiService.Endpoints;

/// <summary>
/// Handles all order-related endpoints (get orders, enriched orders)
/// These are gateway endpoints that call the Order Service microservice
/// </summary>
public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders")
            .RequireAuthorization();

        group.MapGet("/", HandleGetOrders)
            .WithName("GetOrders")
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/enrich", HandleGetEnrichedOrders)
            .WithName("GetEnrichedOrders")
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleGetOrders(IMicroserviceClient microserviceClient)
    {
        try
        {
            var orders = await microserviceClient.GetOrdersAsync();
            return Results.Ok(orders);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> HandleGetEnrichedOrders(IMicroserviceClient microserviceClient)
    {
        try
        {
            var enrichedOrders = await microserviceClient.GetEnrichedOrdersAsync();
            return Results.Ok(enrichedOrders);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}

