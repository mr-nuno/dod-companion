using DodCompanion.Application.Common.Interfaces;
using DodCompanion.Infrastructure.Persistence;
using DodCompanion.Infrastructure.Search;
using DodCompanion.Infrastructure.Time;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace DodCompanion.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the persistence store + seam, the database initializer, the clock, and the external
    /// Rules search client (typed HttpClient + resilience + caching). Application wiring is registered separately.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        AddPersistence(services, configuration);
        AddRulesSearch(services, configuration);

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RavenSettings>(configuration.GetSection(RavenSettings.SectionName));

        services.AddSingleton<IDocumentStore>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<RavenSettings>>().Value;
            var store = new DocumentStore { Urls = settings.Urls, Database = settings.DatabaseName };
            RavenConventions.ApplyConventions(store);
            return store.Initialize();
        });

        // One async session per request = one unit of work; the store stays a singleton.
        services.AddScoped(sp => sp.GetRequiredService<IDocumentStore>().OpenAsyncSession());
        services.AddScoped<IApplicationDbContext, RavenDbContext>();
        services.AddHostedService<RavenDatabaseInitializer>();
    }

    private static void AddRulesSearch(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RulesApiOptions>(configuration.GetSection(RulesApiOptions.SectionName));

        // Token acquisition: a bare client for the OIDC token endpoint + a singleton caching provider.
        services.AddHttpClient(RulesTokenProvider.HttpClientName);
        services.AddSingleton<IRulesTokenProvider, RulesTokenProvider>();
        services.AddTransient<RulesAuthHandler>();

        services.AddHttpClient<RulesSearchClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<RulesApiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        })
        .AddHttpMessageHandler<RulesAuthHandler>() // attaches the client-credentials bearer token
        .AddStandardResilienceHandler();

        services.AddMemoryCache();

        // Handlers depend on the cached interface, never the raw typed client.
        services.AddScoped<IRulesSearchClient>(sp => new CachedRulesSearchClient(
            sp.GetRequiredService<RulesSearchClient>(),
            sp.GetRequiredService<IMemoryCache>(),
            sp.GetRequiredService<IOptions<RulesApiOptions>>()));
    }
}
