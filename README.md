# DoD Companion

A TTRPG session companion built as a **Backend-for-Frontend (Confidential Client)**: a .NET BFF
holds all secrets and state; the React frontend never sees API keys or bearer tokens.

## Features

1. **Host-gated rooms** — a host provisions a room with a room name + the shared **host key**
   (`Sessions:CreateKey`); this generates an unguessable join token and shows a QR code. The host then
   enters a player name to join, exactly like everyone else — players join only by scanning the QR /
   invite link (`/?join=<token>`), never by guessing a room name. The BFF issues an HttpOnly cookie on
   join.
2. **Shared timeline** — players post log entries persisted to RavenDB; everyone sees a live feed
   pushed over **SignalR** (with an initial REST load).
3. **Rule search proxy** — the BFF forwards rule questions to an external PDF Search API, attaching a
   secret JWT Bearer token server-side. Markdown results are rendered with `react-markdown`.

## Stack

- **Backend:** .NET 10, FastEndpoints, MediatR 12.4.1, FluentValidation, Ardalis.Result, RavenDB
  (document-session seam), SignalR, Serilog, Scalar docs.
- **Frontend:** React + TypeScript (strict), Vite, Tailwind CSS, RxJs (`useObservable` hooks),
  `@microsoft/signalr`, `react-markdown`.
- **Tests:** xUnit + Shouldly + NSubstitute (unit); `WebApplicationFactory` against a real RavenDB
  (integration).

## Project layout

```
backend/   # .NET solution (Domain / Application / Infrastructure / Api + tests)
frontend/  # Vite React app (built into the API's wwwroot for Docker)
Dockerfile # single image: the API serves the built SPA from wwwroot
docker-compose.yml
```

## Run with Docker

```bash
# Put the OAuth client secret in backend/.env (loaded into the api container via env_file).
cp backend/.env.example backend/.env
# edit backend/.env -> RulesApi__Auth__ClientSecret=<your-client-secret>

docker compose up --build
```

`backend/.env` is gitignored and optional — the stack still starts without it (rule search just
degrades gracefully until the secret is provided).

A single image is built from the root `Dockerfile`: a Node stage builds the SPA, a .NET stage
publishes the API, and the SPA's build output is copied into the API's `wwwroot`. The API then
serves the SPA and the API endpoints from the same origin — no nginx, no reverse proxy.

- App (SPA + API): http://localhost:8081 · RavenDB Studio: http://localhost:8080

## Run locally (without Docker)

```bash
# 1. RavenDB (unsecured, dev only)
docker run -d -p 8080:8080 \
  -e RAVEN_Setup_Mode=None -e RAVEN_License_Eula_Accepted=true \
  -e RAVEN_Security_UnsecuredAccessAllowed=PublicNetwork ravendb/ravendb:latest

# 2. The BFF fetches the search-API token via OAuth2 client-credentials.
#    Only the client secret is sensitive — put it in user-secrets (never committed).
cd backend/src/DodCompanion.Api
dotnet user-secrets init
dotnet user-secrets set "RulesApi:Auth:ClientSecret" "<your-client-secret>"
#    The host key gates room creation. Development defaults to "dev-host-key"
#    (appsettings.Development.json); override it via user-secrets / env if you like.
dotnet user-secrets set "Sessions:CreateKey" "<your-host-key>"

# 3. Backend (http://localhost:5116)
dotnet run

# 4. Frontend (http://localhost:5173)
#    Reads the BFF URL from frontend/.env.development (VITE_API_BASE_URL=http://localhost:5116).
cd ../../../frontend && npm install && npm run dev
```

## Tests

```bash
cd backend && dotnet test
```

Integration tests target a RavenDB at `RAVEN_TEST_URL` (default `http://localhost:8080`) and are
skipped if it is unreachable.

## Configuration

| Key | Where | Notes |
|---|---|---|
| `Raven:Urls` / `Raven:DatabaseName` | appsettings / env | RavenDB connection |
| `RulesApi:BaseUrl` | appsettings | external Search API base URL |
| `RulesApi:Auth:TokenEndpoint` / `ClientId` / `GrantType` | appsettings | OAuth2 client-credentials config |
| `RulesApi:Auth:ClientSecret` | **user-secrets / env** (`RULES_API_CLIENT_SECRET`) | OAuth client secret — never committed |
| `Cors:AllowedOrigins` | appsettings / env | allowed SPA origins |
| `Sessions:CreateKey` | **user-secrets / env** (`Sessions__CreateKey`) | shared host key required to create rooms; if empty, room creation is disabled (fails closed) |

The BFF acquires the Rules API bearer token from `RulesApi:Auth:TokenEndpoint` using
`client_credentials`, caches it until shortly before expiry, and attaches it to every search request
via a delegating handler. The frontend never sees the token or the secret.
