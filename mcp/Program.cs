using DodMcpServer.Rules;
using DodMcpServer.Tools;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using ModelContextProtocol.AspNetCore.Authentication;
using ModelContextProtocol.Authentication;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, _, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// --- Outbound auth: OAuth2 client-credentials token for the external Rules API ---
builder.Services.Configure<RulesApiOptions>(builder.Configuration.GetSection(RulesApiOptions.SectionName));

// A bare client for the OIDC token endpoint + a singleton caching provider.
builder.Services.AddHttpClient(RulesTokenProvider.HttpClientName);
builder.Services.AddSingleton<IRulesTokenProvider, RulesTokenProvider>();
builder.Services.AddTransient<RulesAuthHandler>();

// The client the tool uses: base address = RulesApi:BaseUrl, bearer token attached per request.
builder.Services.AddHttpClient(DodRulesTool.HttpClientName, (sp, client) =>
    {
        var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RulesApiOptions>>().Value;
        client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    })
    .AddHttpMessageHandler<RulesAuthHandler>()
    .AddStandardResilienceHandler();

// --- Inbound auth: OAuth 2.0 resource server. JwtBearer validates Logto tokens; the MCP
// authentication scheme advertises the authorization server (Logto) via RFC 9728 protected-resource
// metadata and challenges unauthenticated callers so MCP clients can run the OAuth flow themselves. ---
var logtoAuthority = builder.Configuration["Logto:Authority"]
    ?? throw new InvalidOperationException("Logto:Authority is required.");
var mcpResource = builder.Configuration["Logto:Audience"]
    ?? throw new InvalidOperationException("Logto:Audience is required (the MCP API resource identifier).");
var scopesSupported = builder.Configuration.GetSection("Logto:Scopes").Get<string[]>() ?? ["api:read"];

builder.Services.AddAuthentication(options =>
    {
        options.DefaultChallengeScheme = McpAuthenticationDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.Authority = logtoAuthority;
        options.Audience = mcpResource;
    })
    .AddMcp(options =>
    {
        options.ResourceMetadata = new ProtectedResourceMetadata
        {
            Resource = mcpResource,
            AuthorizationServers = { logtoAuthority },
            ScopesSupported = [.. scopesSupported],
        };
    });

builder.Services.AddAuthorization();

// --- Behind a TLS-terminating reverse proxy: honor X-Forwarded-Proto/Host so the OAuth
// discovery + WWW-Authenticate `resource_metadata` URLs are rebuilt with the external https host
// instead of the container's internal http://…:8080. ---
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    // The container is only reachable via the trusted reverse proxy, so accept forwarded
    // headers from any upstream (the default KnownNetworks/KnownProxies would drop them).
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// --- MCP server over HTTP/SSE transport, exposing the single tool ---
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<DodRulesTool>();

var app = builder.Build();

// Must run first so scheme/host are corrected before auth and endpoints read them.
app.UseForwardedHeaders();

app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

// Liveness probe (public — no token required).
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

// MCP endpoint, protected by the Logto JWT.
app.MapMcp("/sse").RequireAuthorization();

app.Run();
