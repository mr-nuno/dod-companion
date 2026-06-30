using System.Text;
using DodCompanion.Infrastructure.Search;
using Microsoft.Extensions.Configuration;
using Shouldly;

namespace DodCompanion.UnitTests.Infrastructure;

/// <summary>
/// Binds <see cref="RulesApiOptions"/> through the real configuration binder. This is the path that
/// production uses (services.Configure&lt;RulesApiOptions&gt;) — and the one the in-memory option tests
/// bypass, which is why a binding regression in PageModifiers slipped through to production.
/// </summary>
public class RulesApiOptionsTests
{
    private static RulesApiOptions Bind(string json) =>
        new ConfigurationBuilder()
            .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)))
            .Build()
            .GetSection(RulesApiOptions.SectionName)
            .Get<RulesApiOptions>()!;

    [Fact]
    public void Binds_PageModifiers_From_Configuration()
    {
        const string json = """
        {
          "RulesApi": {
            "BaseUrl": "https://rules.test",
            "PageModifiers": { "DoD_Regler_v2-1.pdf": -2 }
          }
        }
        """;

        var options = Bind(json);

        options.PageModifiers.ShouldContainKeyAndValue("DoD_Regler_v2-1.pdf", -2);
    }

    [Fact]
    public void Bound_PageModifiers_Lookup_Is_CaseInsensitive()
    {
        const string json = """
        { "RulesApi": { "PageModifiers": { "DoD_Regler_v2-1.pdf": -2 } } }
        """;

        var options = Bind(json);

        options.PageModifiers.TryGetValue("dod_regler_v2-1.PDF", out var modifier).ShouldBeTrue();
        modifier.ShouldBe(-2);
    }

    [Fact]
    public void Bound_PageModifiers_Defaults_To_Empty_When_Absent()
    {
        var options = Bind("""{ "RulesApi": { "BaseUrl": "https://rules.test" } }""");

        options.PageModifiers.ShouldBeEmpty();
    }
}
