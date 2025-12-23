using NewGenewizPOC.ApiService.Endpoints;
using NewGenewizPOC.ApiService.Extensions;
using NewGenewizPOC.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// ============= CONFIGURATION =============
builder.AddServiceDefaults();

// ============= SERVICES SETUP =============
builder.Services
    .AddAuthenticationServices(builder.Configuration)
    .AddCorsPolicy(builder.Configuration)
    .AddMicroserviceClient();

// ============= BUILD THE APP =============
var app = builder.Build();

// ============= MIDDLEWARE PIPELINE =============
app.MapDefaultEndpoints();
app.UseAuthenticationMiddleware();

// ============= ENDPOINT MAPPING =============
app.MapAuthenticationEndpoints();
app.MapOrderEndpoints();

// ============= RUN =============
await app.RunAsync();
