namespace DodCompanion.Domain.Common;

/// <summary>
/// Marker interface for aggregate roots. Other aggregates reference a root by id, never by object reference.
/// </summary>
public interface IAggregateRoot;
