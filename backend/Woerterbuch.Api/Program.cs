using System.Text.Json.Serialization;
using Woerterbuch.Api.Endpoints;
using Woerterbuch.Api.Middleware;
using Woerterbuch.Api.Startup;
using Woerterbuch.Core;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddWoerterbuchDb(builder.Configuration);
builder.Services.AddOpenApi();
// No reading numbers from strings, so OpenAPI describes numbers as number rather than number | string.
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.NumberHandling = JsonNumberHandling.Strict);
builder.Services.AddValidation();
builder.Services.AddProblemDetails();

var app = builder.Build();

await app.MigrateDatabaseAsync();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseExceptionHandler();

app.UseMiddleware<RequestAbortedMiddleware>();

app.MapDefaultEndpoints();

var api = app.MapGroup("/api");
api.MapSearchEndpoints();
api.MapWordEndpoints();

app.Run();
