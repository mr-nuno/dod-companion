using Raven.Client.Documents.Linq;

namespace DodCompanion.Application.Common.Interfaces;

/// <summary>
/// Persistence seam over the RavenDB async document session. One session = one unit of work per request.
/// Handlers depend on this, never on <c>IDocumentStore</c> / <c>IAsyncDocumentSession</c> directly.
/// Exposes RavenDB's native query type exactly as an EF seam would expose <c>DbSet&lt;T&gt;</c>.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>Start a LINQ query against the collection for <typeparamref name="T"/>.</summary>
    IRavenQueryable<T> Query<T>();

    /// <summary>Load a single document by its string id, or null when not found.</summary>
    Task<T?> LoadAsync<T>(string id, CancellationToken ct);

    /// <summary>Stage a new document for insertion; RavenDB assigns its id here.</summary>
    Task StoreAsync<T>(T entity, CancellationToken ct) where T : class;

    /// <summary>Flush all staged changes as a single unit of work.</summary>
    Task SaveChangesAsync(CancellationToken ct);
}
