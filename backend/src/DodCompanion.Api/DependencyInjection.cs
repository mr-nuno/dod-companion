using DodCompanion.Api.Auth;
using DodCompanion.Api.Hubs;
using DodCompanion.Application.Common.Interfaces;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace DodCompanion.Api;

public static class DependencyInjection
{
    public const string CorsPolicyName = "frontend";

    /// <summary>Registers API-layer services: auth (cookie), CORS, SignalR, FastEndpoints, and the seams it implements.</summary>
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserSession, CookieUserSession>();
        services.AddSingleton<ITimelineNotifier, TimelineNotifier>();

        AddCookieAuth(services);
        AddCorsPolicy(services, configuration);

        services.AddSignalR();

        services.AddFastEndpoints()
            .SwaggerDocument(o => o.DocumentSettings = s => s.Title = "DoD Companion BFF");

        return services;
    }

    private static void AddCookieAuth(IServiceCollection services)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "dod.session";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.ExpireTimeSpan = TimeSpan.FromHours(12);
                options.SlidingExpiration = true;

                // This is an API, not an MVC app — answer with status codes instead of redirecting to a login page.
                options.Events.OnRedirectToLogin = ctx =>
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = ctx =>
                {
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            });

        services.AddAuthorization();
    }

    private static void AddCorsPolicy(IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options =>
            options.AddPolicy(CorsPolicyName, policy =>
                policy.WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()));
    }
}
