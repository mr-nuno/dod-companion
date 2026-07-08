using DodCompanion.Application.Common.Interfaces;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace DodCompanion.Infrastructure.Persistence;

/// <summary>
/// Implements the persistence seam over a scoped RavenDB async session. The session *is* the unit of work.
/// </summary>
public sealed class RavenDbContext(IAsyncDocumentSession session) : IApplicationDbContext
{
    public IRavenQueryable<T> Query<T>() => session.Query<T>();

    public async Task<T?> LoadAsync<T>(string id, CancellationToken ct) =>
        await session.LoadAsync<T>(id, ct);

    public Task StoreAsync<T>(T entity, CancellationToken ct) where T : class =>
        session.StoreAsync(entity, ct);

    public void Delete<T>(T entity) where T : class => session.Delete(entity);

    public Task SaveChangesAsync(CancellationToken ct) => session.SaveChangesAsync(ct);
}
