namespace DodCompanion.Application.Common.Dtos;

/// <summary>
/// The proxied rule-search payload returned to the frontend. Mirrors the external
/// Search API's <c>data</c> object (minus the BFF-internal envelope).
/// </summary>
public sealed record RuleSearchResult(
    string Query,
    int TotalHits,
    IReadOnlyList<RuleSearchHit> Results);

/// <summary>A single matched PDF page. <see cref="Content"/> is Markdown, rendered client-side.</summary>
public sealed record RuleSearchHit(
    string SourceFileName,
    int PhysicalPageNumber,
    string? Header,
    string Content,
    IReadOnlyList<string> Tags,
    double SearchScore);
