using DodCompanion.Api;
using DodCompanion.Api.Hubs;
using DodCompanion.Application;
using DodCompanion.Application.Common.Models;
using DodCompanion.Infrastructure;
using FastEndpoints;
using FastEndpoints.Swagger;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, _, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration)
    .AddApiServices(builder.Configuration);

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail("An unexpected error occurred."));
}));

// Minimal security headers.
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});

// Serve the React SPA from wwwroot. Static assets are public and resolved before auth,
// so the bundle loads without a session cookie.
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors(DodCompanion.Api.DependencyInjection.CorsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpoints(c =>
{
    // Authenticated by default; endpoints opt out with AllowAnonymous().
    c.Endpoints.Configurator = ep => ep.Options(b => b.RequireAuthorization());

    // Validation failures return the standard ApiResponse envelope, not ProblemDetails.
    c.Errors.ResponseBuilder = (failures, _, _) =>
        ApiResponse<object>.Invalid(failures
            .Select(f => new ValidationError(f.PropertyName, f.ErrorMessage))
            .ToList());
});

app.MapHub<TimelineHub>("/hubs/timeline").RequireCors(DodCompanion.Api.DependencyInjection.CorsPolicyName);
app.MapHealthChecks("/health");

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
    app.MapScalarApiReference(o => o.WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json"));
}

// SPA fallback: client-side routes (anything not matched by an API endpoint or static file)
// resolve to index.html. Registered last so real endpoints always win.
app.MapFallbackToFile("index.html");

app.Run();

// Exposed for WebApplicationFactory in integration tests.
public partial class Program;
