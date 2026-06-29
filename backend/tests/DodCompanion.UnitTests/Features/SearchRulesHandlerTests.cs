using Ardalis.Result;
using DodCompanion.Application.Common.Dtos;
using DodCompanion.Application.Common.Interfaces;
using DodCompanion.Application.Features.Rules.SearchRules;
using NSubstitute;
using Shouldly;

namespace DodCompanion.UnitTests.Features;

public class SearchRulesHandlerTests
{
    private readonly IRulesSearchClient _searchClient = Substitute.For<IRulesSearchClient>();

    [Fact]
    public async Task Handle_Should_TrimQueryAndPassThroughResult()
    {
        var expected = new RuleSearchResult("combat", 1, []);
        _searchClient.SearchAsync("combat", Arg.Any<CancellationToken>()).Returns(Result.Success(expected));

        var handler = new SearchRulesQuery.Handler(_searchClient);
        var result = await handler.Handle(new SearchRulesQuery("  combat  "), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expected);
        await _searchClient.Received(1).SearchAsync("combat", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_PropagateError_When_ClientFails()
    {
        _searchClient.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result<RuleSearchResult>.Error("upstream down"));

        var handler = new SearchRulesQuery.Handler(_searchClient);
        var result = await handler.Handle(new SearchRulesQuery("combat"), CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Error);
    }
}
