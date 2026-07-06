# DoD MCP Server

A [Model Context Protocol](https://modelcontextprotocol.io) server that exposes the Drakar och Demoner
rules to an LLM as a single tool. It speaks MCP over **HTTP/SSE** (so it can be hosted at a URL), is
protected by **Logto** JWTs on the way in, and proxies to the external DoD PDF Search API on the way out.

- **Transport:** SSE over HTTP, mapped at `/sse`.
- **Tool:** `search_dod_rules` — searches the rulebooks for rules, monster stats, magic, and equipment.
- **Inbound auth:** OAuth 2.0 / JWT bearer, validated against Logto (`Logto:Authority` + `Logto:Audience`).
- **Outbound auth:** the server obtains its own OAuth2 **client-credentials** token from Logto and attaches
  it to every call to the Rules API — clients never see it.

## Two auth layers

| | Direction | Mechanism | Config |
|---|---|---|---|
| **Inbound** | MCP client → `/sse` | JWT bearer validated against Logto | `Logto:Authority`, `Logto:Audience` |
| **Outbound** | server → Rules API `/search` | client-credentials bearer (cached) | `RulesApi:Auth:*` |

The `search_dod_rules` tool returns the **raw JSON** from the Rules API as text, so the LLM sees the full
`{ "success": true, "data": { "results": [ ... ] } }` envelope (each result's `content` is Markdown).

## Configuration

`appsettings.json` holds a template; secrets stay empty and are supplied via environment variables
(ASP.NET `__` section separator) or user-secrets.

| Key | Example | Notes |
|---|---|---|
| `Logto:Authority` | `https://auth.pewi.se/oidc` | Logto issuer URL |
| `Logto:Audience` | `<your MCP API resource id>` | The Logto API resource this server represents |
| `RulesApi:BaseUrl` | `https://pdf-search-api-1-0-0.onrender.com` | External Rules API |
| `RulesApi:Auth:ClientSecret` | *(secret)* | **Never commit.** Set via env / user-secrets. |

### Prerequisite (Logto setup)

1. Create a Logto **API resource** for this server and use its identifier as `Logto:Audience`.
2. Give MCP clients a way to obtain a token for that audience (an app / M2M credential in Logto).
3. For the outbound call, either reuse the backend BFF's existing client-credentials app or create one for
   this server, then set its secret in `RulesApi:Auth:ClientSecret`.

## Run locally

```bash
# Provide the outbound secret without committing it.
dotnet user-secrets set "RulesApi:Auth:ClientSecret" "<secret>"   # first run: dotnet user-secrets init
# (or) export RulesApi__Auth__ClientSecret="<secret>"

dotnet run
```

The server listens on the Kestrel default (dev) or `http://+:8080` in the container.

- Health check: `GET /health` → `{ "status": "ok" }` (public, no token).
- MCP endpoint: `GET /sse` → `401` without a valid bearer token.

## Build & run with Docker

Build once, run anywhere. The image listens on port **8080**.

```bash
# From this directory:
docker build -t dod-mcp-server .

docker run --rm -p 8082:8080 \
  -e Logto__Audience="<your MCP API resource id>" \
  -e RulesApi__Auth__ClientSecret="<secret>" \
  dod-mcp-server
```

Or via the repo-root compose file (service `mcp`, published on host port **8082**):

```bash
cp mcp/.env.example mcp/.env   # then fill in RulesApi__Auth__ClientSecret and Logto__Audience
docker compose up mcp
```

## Connecting a client (OAuth)

The server is an OAuth 2.0 **resource server**. It publishes
`/.well-known/oauth-protected-resource` (RFC 9728) pointing at Logto, and challenges unauthenticated
callers with `WWW-Authenticate: Bearer resource_metadata="…"`. A compliant MCP client discovers Logto
from that, runs the authorization-code + PKCE flow itself, and auto-refreshes — no manual tokens.

**Logto does not support Dynamic Client Registration**, so the client must use a **pre-registered app**.

### One-time Logto setup

1. Create an **application** for the MCP client (a public/native app using PKCE, or a traditional web
   app if you prefer a client secret).
2. Add the redirect URI the client uses — for Claude Code: `http://localhost:<callback-port>/callback`
   (pick a fixed port, e.g. `8090`).
3. Grant the app/user access to the API resource (`Logto:Audience`) with the `api:read` scope, and
   enable **refresh tokens** (`offline_access`) so sessions renew without re-login.

### Claude Code

Because Logto has no DCR, pass the pre-registered client id and a fixed callback port. Add to
`.mcp.json` (or use the equivalent `claude mcp add` flags):

```json
{
  "mcpServers": {
    "dod-rules": {
      "type": "http",
      "url": "http://localhost:8082/sse",
      "oauth": {
        "clientId": "<logto-app-client-id>",
        "callbackPort": 8090,
        "scopes": "openid offline_access api:read"
      }
    }
  }
}
```

Then run `/mcp` in Claude Code, authenticate in the browser, and the `search_dod_rules` tool appears.

### Manual bearer token (fallback / scripting)

Any valid Logto access token with `aud` = `Logto:Audience` also works — attach it directly:

```
GET /sse HTTP/1.1
Authorization: Bearer <logto-access-token>
```

Mint one via client-credentials (M2M app authorized for the resource):

```bash
curl -s https://auth.pewi.se/oidc/token \
  -d grant_type=client_credentials \
  -d client_id=<m2m-client-id> \
  --data-urlencode client_secret=<secret> \
  --data-urlencode resource=<your MCP API resource id>

# register with the token as a static header:
claude mcp add --transport http dod-rules http://localhost:8082/sse \
  --header "Authorization: Bearer <token>"
```
