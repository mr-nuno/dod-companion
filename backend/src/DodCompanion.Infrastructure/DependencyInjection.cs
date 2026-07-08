using System.Security.Cryptography.X509Certificates;
using DodCompanion.Application.Common.Interfaces;
using DodCompanion.Infrastructure.Email;
using DodCompanion.Infrastructure.Persistence;
using DodCompanion.Infrastructure.Search;
using DodCompanion.Infrastructure.Security;
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
        AddEmail(services, configuration);

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<ITokenGenerator, CryptoTokenGenerator>();

        return services;
    }

    private static void AddEmail(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ResendOptions>(configuration.GetSection(ResendOptions.SectionName));

        services.AddHttpClient<IEmailSender, ResendEmailSender>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ResendOptions>>().Value;
            client.BaseAddress = new Uri("https://api.resend.com");
            if (!string.IsNullOrWhiteSpace(options.ApiKey))
            {
                client.DefaultRequestHeaders.Authorization = new("Bearer", options.ApiKey);
            }
        })
        .AddStandardResilienceHandler();
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RavenSettings>(configuration.GetSection(RavenSettings.SectionName));

        services.AddSingleton<IDocumentStore>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<RavenSettings>>().Value;
            var store = new DocumentStore
            {
                Urls = settings.Urls,
                Database = settings.DatabaseName,
                Certificate = LoadCertificate(settings),
            };
            RavenConventions.ApplyConventions(store);
            return store.Initialize();
        });

        // One async session per request = one unit of work; the store stays a singleton.
        services.AddScoped(sp => sp.GetRequiredService<IDocumentStore>().OpenAsyncSession());
        services.AddScoped<IApplicationDbContext, RavenDbContext>();
        services.AddHostedService<RavenDatabaseInitializer>();
    }

    // A secured RavenDB cluster (e.g. RavenDB Cloud over https) authenticates the client with an
    // X.509 client certificate; an unsecured local store needs none. Returning null leaves the store
    // unauthenticated.
    //
    // Resolution order:
    //   1. RAVENDB_CERT_BASE64 env var — base64-encoded PFX, used in production (no file on disk).
    //   2. Raven:CertificatePath config — file path fallback for local overrides.
    private static X509Certificate2? LoadCertificate(RavenSettings settings)
    {
        var base64 = Environment.GetEnvironmentVariable("RAVENDB_CERT_BASE64");
        if (!string.IsNullOrWhiteSpace(base64))
            return X509CertificateLoader.LoadPkcs12(Convert.FromBase64String(base64), password: null);

        if (string.IsNullOrWhiteSpace(settings.CertificatePath))
            return null;

        if (!File.Exists(settings.CertificatePath))
            throw new FileNotFoundException(
                $"RavenDB client certificate not found at '{settings.CertificatePath}'.", settings.CertificatePath);

        return X509CertificateLoader.LoadPkcs12FromFile(
            settings.CertificatePath,
            string.IsNullOrEmpty(settings.CertificatePassword) ? null : settings.CertificatePassword);
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
