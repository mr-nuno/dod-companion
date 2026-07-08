using DodCompanion.Domain.LogEntry;
using DodCompanion.Domain.Session;
using DodCompanion.Domain.SessionLink;
using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;

namespace DodCompanion.Infrastructure.Persistence;

/// <summary>
/// The single source of truth for store conventions (collection naming, id separator).
/// Applied identically by the production store and the integration-test store — tests never re-derive these.
/// </summary>
public static class RavenConventions
{
    public static void ApplyConventions(IDocumentStore store)
    {
        store.Conventions.IdentityPartsSeparator = '/';

        store.Conventions.FindCollectionName = type =>
        {
            if (type == typeof(SessionAggregate))
            {
                return "Sessions";
            }

            if (type == typeof(LogEntryAggregate))
            {
                return "LogEntries";
            }

            if (type == typeof(SessionLinkAggregate))
            {
                return "SessionLinks";
            }

            return DocumentConventions.DefaultGetCollectionName(type);
        };
    }
}
