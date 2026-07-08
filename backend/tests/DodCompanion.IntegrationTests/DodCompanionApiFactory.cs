using System.Collections.Concurrent;
using System.Text.RegularExpressions;
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

    /// <summary>The allowlisted Game Master email the test factory configures so tests can create rooms.</summary>
    public const string AllowedEmail = "sl@example.com";

    /// <summary>Captures the magic links that would have been emailed, so tests can consume them.</summary>
    public CapturingEmailSender Emails { get; } = new();

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.UseSetting("Raven:Urls:0", RavenUrl);
        builder.UseSetting("Raven:DatabaseName", DatabaseName);
        builder.UseSetting("Cors:AllowedOrigins:0", "http://localhost");
        builder.UseSetting("Sessions:AllowedDmEmails:0", AllowedEmail);
        builder.UseSetting("Sessions:AppBaseUrl", "http://localhost");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IRulesSearchClient>();
            services.AddScoped<IRulesSearchClient, StubRulesSearchClient>();

            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(Emails);
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

    /// <summary>Test double for <see cref="IEmailSender"/> that captures the magic-link token from each email.</summary>
    public sealed class CapturingEmailSender : IEmailSender
    {
        private readonly ConcurrentQueue<string> _tokens = new();

        public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct)
        {
            var match = Regex.Match(htmlBody, @"/create\?token=([^""&]+)");
            if (match.Success)
            {
                _tokens.Enqueue(match.Groups[1].Value);
            }

            return Task.CompletedTask;
        }

        /// <summary>Pops the token from the oldest captured email.</summary>
        public string DequeueToken() =>
            _tokens.TryDequeue(out var token) ? token : throw new InvalidOperationException("No magic link was captured.");
    }

    private sealed class StubRulesSearchClient : IRulesSearchClient
    {
        public Task<Result<RuleSearchResult>> SearchAsync(string query, CancellationToken ct) =>
            Task.FromResult(Result.Success(new RuleSearchResult(
                query,
                null,
                1,
                [new RuleSearchHit("rulebook.pdf", 42, "Combat Rules", "**Roll initiative**", ["combat"], 0.95, 0)])));
    }
}
