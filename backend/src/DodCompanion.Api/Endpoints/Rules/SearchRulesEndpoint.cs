using DodCompanion.Application.Common.Dtos;
using DodCompanion.Application.Common.Models;
using DodCompanion.Application.Features.Rules.SearchRules;
using FastEndpoints;
using FluentValidation;
using MediatR;

namespace DodCompanion.Api.Endpoints.Rules;

/// <summary>Query string for the rules search proxy: <c>?query={term}</c>.</summary>
public sealed class SearchRulesRequest
{
    [QueryParam]
    public string Query { get; init; } = string.Empty;
}

/// <summary>GET /rules/search — proxies a rule question to the external Search API (token attached server-side).</summary>
public sealed class SearchRulesEndpoint(ISender sender) : Endpoint<SearchRulesRequest, ApiResponse<RuleSearchResult>>
{
    public override void Configure()
    {
        Get("/rules/search");
        Summary(s =>
        {
            s.Summary = "Search the rulebooks via the external PDF Search API.";
            s.Description = "The BFF forwards the query with its secret Bearer token. Results contain Markdown content.";
            s.Params["query"] = "The full-text search term.";
        });
        Tags("Rules");
    }

    public override async Task HandleAsync(SearchRulesRequest req, CancellationToken ct)
    {
        var result = await sender.Send(new SearchRulesQuery(req.Query), ct);
        await Send.ResponseAsync(result.ToApiResponse(), result.ToHttpStatusCode(), ct);
    }
}

/// <summary>HTTP-level validation for the rules search request.</summary>
public sealed class SearchRulesRequestValidator : Validator<SearchRulesRequest>
{
    public SearchRulesRequestValidator()
    {
        RuleFor(x => x.Query).NotEmpty().WithMessage("A search query is required.");
    }
}
