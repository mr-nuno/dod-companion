using Ardalis.Result;
using DodCompanion.Application.Common.Dtos;
using DodCompanion.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace DodCompanion.Application.Features.Rules.SearchRules;

/// <summary>
/// Proxies a rule question to the external Search API via <see cref="IRulesSearchClient"/>,
/// which attaches the secret Bearer token. The frontend never sees the token.
/// </summary>
public sealed record SearchRulesQuery(string Query) : IRequest<Result<RuleSearchResult>>
{
    public sealed class Handler(IRulesSearchClient searchClient)
        : IRequestHandler<SearchRulesQuery, Result<RuleSearchResult>>
    {
        public Task<Result<RuleSearchResult>> Handle(SearchRulesQuery request, CancellationToken ct) =>
            searchClient.SearchAsync(request.Query.Trim(), ct);
    }

    public sealed class Validator : AbstractValidator<SearchRulesQuery>
    {
        public Validator()
        {
            RuleFor(x => x.Query)
                .NotEmpty().WithMessage("A search query is required.")
                .MaximumLength(256).WithMessage("The search query must be 256 characters or fewer.");
        }
    }
}
