using System.ComponentModel;
using ModelContextProtocol.Server;

namespace DodMcpServer.Tools;

/// <summary>
/// The single MCP tool this server exposes: a thin, defensive proxy over the external DoD PDF Search API.
/// The outbound bearer token is attached by the <c>RulesApi</c> HttpClient's message handler — it never
/// appears here. On success the raw upstream JSON is returned verbatim as text (per the MCP contract);
/// on any failure a clear message is returned so the LLM understands what went wrong, and the server
/// keeps running.
/// </summary>
[McpServerToolType]
public sealed class DodRulesTool
{
    /// <summary>Name of the configured HttpClient pointed at <c>RulesApi:BaseUrl</c> with the auth handler.</summary>
    public const string HttpClientName = "RulesApi";

    [McpServerTool(Name = "search_dod_rules")]
    [Description("Söker i regelverket för Drakar och Demoner. Används för att hitta regler, stats för monster, magi eller utrustning.")]
    public static async Task<string> SearchDodRules(
        IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory,
        [Description("Söktermen att slå upp i regelverket, t.ex. ett monsternamn, en besvärjelse eller en regel.")] string query,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger<DodRulesTool>();

        if (string.IsNullOrWhiteSpace(query))
        {
            return "Error: a non-empty 'query' is required.";
        }

        var requestUri = $"search?query={Uri.EscapeDataString(query)}";

        try
        {
            var client = httpClientFactory.CreateClient(HttpClientName);
            using var response = await client.GetAsync(requestUri, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Rules search returned {StatusCode} for query {Query}", (int)response.StatusCode, query);
                return $"The rules service returned status {(int)response.StatusCode} ({response.ReasonPhrase}).";
            }

            // Return the raw JSON body verbatim as the tool's text result.
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw; // genuine caller cancellation — let it propagate
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Rules search failed for query {Query}", query);
            return "The rules service is currently unavailable. Please try again later.";
        }
    }
}
