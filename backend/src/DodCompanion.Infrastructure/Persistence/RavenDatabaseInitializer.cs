using Microsoft.Extensions.Hosting;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Exceptions;
using Raven.Client.Exceptions.Database;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using Serilog;

namespace DodCompanion.Infrastructure.Persistence;

/// <summary>
/// Startup hosted service: ensures the configured database exists and registers any static indexes.
/// </summary>
public sealed class RavenDatabaseInitializer(IDocumentStore store) : IHostedService
{
    private static readonly ILogger Log = Serilog.Log.ForContext<RavenDatabaseInitializer>();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await EnsureDatabaseExistsAsync(cancellationToken);
        await IndexCreation.CreateIndexesAsync(typeof(RavenDatabaseInitializer).Assembly, store, token: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task EnsureDatabaseExistsAsync(CancellationToken ct)
    {
        var database = store.Database;

        try
        {
            await store.Maintenance.ForDatabase(database).SendAsync(new Raven.Client.Documents.Operations.GetStatisticsOperation(), ct);
            return;
        }
        catch (DatabaseDoesNotExistException)
        {
            // Fall through to create it.
        }

        try
        {
            await store.Maintenance.Server.SendAsync(new CreateDatabaseOperation(new DatabaseRecord(database)), ct);
            Log.Information("Created RavenDB database {Database}", database);
        }
        catch (ConcurrencyException)
        {
            // Another instance created it first — fine.
        }
    }
}
