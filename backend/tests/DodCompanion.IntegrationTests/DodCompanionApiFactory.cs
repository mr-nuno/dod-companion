using Ardalis.Result;
using DodCompanion.Application.Common.Dtos;
using DodCompanion.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Raven.Client.Documents;
using Raven.Client.ServerWide.Operations;

namespace DodCompanion.IntegrationTests;

/// <summary>
/// Boots the real BFF against a live RavenDB (the URL in RAVEN_TEST_URL, default http://localhost:8080)
/// using a throwaway database per run. The external search client is stubbed — no network calls.
/// </summary>
public sealed class DodCompanionApiFactory : WebApplicationFactory<Program>
{
    public static readonly string RavenUrl =
        Environment.GetEnvironmentVariable("RAVEN_TEST_URL") ?? "http://localhost:8080";

    public string DatabaseName { get; } = $"DodCompanion_Test_{Guid.NewGuid():N}";

    /// <summary>The host key the test factory configures so tests can create rooms.</summary>
    public const string HostKey = "test-host-key";

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.UseSetting("Raven:Urls:0", RavenUrl);
        builder.UseSetting("Raven:DatabaseName", DatabaseName);
        builder.UseSetting("Cors:AllowedOrigins:0", "http://localhost");
        builder.UseSetting("Sessions:CreateKey", HostKey);

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IRulesSearchClient>();
            services.AddScoped<IRulesSearchClient, StubRulesSearchClient>();
        });
    }

    public static bool RavenIsReachable()
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            var response = client.GetAsync($"{RavenUrl}/").GetAwaiter().GetResult();
            return true && response is not null;
        }
        catch
        {
            return false;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && RavenIsReachable())
        {
            try
            {
                using var store = new DocumentStore { Urls = [RavenUrl], Database = DatabaseName };
                store.Initialize();
                store.Maintenance.Server.Send(new DeleteDatabasesOperation(DatabaseName, hardDelete: true));
            }
            catch
            {
                // best-effort cleanup
            }
        }

        base.Dispose(disposing);
    }

    private sealed class StubRulesSearchClient : IRulesSearchClient
    {
        public Task<Result<RuleSearchResult>> SearchAsync(string query, CancellationToken ct) =>
            Task.FromResult(Result.Success(new RuleSearchResult(
                query,
                null,
                1,
                [new RuleSearchHit("rulebook.pdf", 42, "Combat Rules", "**Roll initiative**", ["combat"], 0.95)])));
    }
}
