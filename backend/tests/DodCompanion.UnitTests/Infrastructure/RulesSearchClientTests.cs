using System.Net;
using System.Text;
using Ardalis.Result;
using DodCompanion.Infrastructure.Search;
using Shouldly;

namespace DodCompanion.UnitTests.Infrastructure;

public class RulesSearchClientTests
{
    private sealed class StubHandler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            });
    }

    private static RulesSearchClient ClientFor(HttpStatusCode status, string body) =>
        new(new HttpClient(new StubHandler(status, body)) { BaseAddress = new Uri("https://rules.test/") });

    [Fact]
    public async Task SearchAsync_Should_MapSuccessfulEnvelope()
    {
        const string body = """
        {
          "success": true,
          "data": {
            "query": "combat",
            "processedQuery": "combat",
            "totalHits": 1,
            "results": [
              {
                "sourceFileName": "rulebook.pdf",
                "physicalPageNumber": 42,
                "header": "Combat Rules",
                "content": "**Roll initiative**",
                "tags": ["combat"],
                "searchScore": 0.95
              }
            ]
          }
        }
        """;

        var result = await ClientFor(HttpStatusCode.OK, body).SearchAsync("combat", CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalHits.ShouldBe(1);
        var hit = result.Value.Results.ShouldHaveSingleItem();
        hit.SourceFileName.ShouldBe("rulebook.pdf");
        hit.PhysicalPageNumber.ShouldBe(42);
        hit.Content.ShouldBe("**Roll initiative**");
        hit.Tags.ShouldContain("combat");
    }

    [Fact]
    public async Task SearchAsync_Should_ReturnError_When_Unauthorized()
    {
        var result = await ClientFor(HttpStatusCode.Unauthorized, "{}").SearchAsync("combat", CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Error);
    }

    [Fact]
    public async Task SearchAsync_Should_ReturnError_When_EnvelopeUnsuccessful()
    {
        const string body = """{ "success": false, "data": null, "error": "boom" }""";

        var result = await ClientFor(HttpStatusCode.OK, body).SearchAsync("combat", CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Error);
        result.Errors.ShouldContain("boom");
    }
}
