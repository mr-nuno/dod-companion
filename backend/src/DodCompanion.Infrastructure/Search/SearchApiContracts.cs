namespace DodCompanion.Infrastructure.Search;

// Internal deserialization shapes mirroring the external Search API envelope.
// Property names map case-insensitively from the API's camelCase JSON.

internal sealed record SearchApiEnvelope(bool Success, SearchApiData? Data, string? Error);

internal sealed record SearchApiData(string Query, string? ProcessedQuery, int TotalHits, List<SearchApiHit>? Results);

internal sealed record SearchApiHit(
    string SourceFileName,
    int PhysicalPageNumber,
    string? Header,
    string Content,
    List<string>? Tags,
    double SearchScore);
