# Projekt: Webbaserad MCP-server för Drakar och Demoner Regel-API

## Mål
Du ska skapa en Model Context Protocol (MCP) server som exponerar ett sökverktyg för ett Drakar och Demoner-regelverk. Servern ska kommunicera via **SSE (Server-Sent Events)** över HTTP/HTTPS så att den kan hostas på en webbadress. API:et ska skyddas med JWT-baserad autentisering via **Logto.io**.

## Teknikstack
* **Ramverk:** .NET 8 (eller senare), ASP.NET Core Minimal API
* **Språk:** C#
* **MCP SDK:** `ModelContextProtocol.AspNetCore` (och `ModelContextProtocol.Server`)
* **Autentisering:** `Microsoft.AspNetCore.Authentication.JwtBearer` (via Logto.io som OIDC-provider)
* **HTTP-klient:** `IHttpClientFactory`
* **Paketering:** Docker

## Kravspecifikation

### 1. Arkitektur och Transport
* Applikationen ska vara ett standard ASP.NET Core webbprojekt (`dotnet new web`).
* Servern MÅSTE konfigureras för HTTP-transport (SSE) med hjälp av `.WithHttpTransport()` från MCP SDK:t.
* MCP-servern ska mappas till en specifik route, förslagsvis `app.MapMcpServer("/sse");`.

### 2. Autentisering (Logto.io)
* Applikationen måste skyddas med OAuth 2.0 / JWT.
* Konfigurera `AddAuthentication().AddJwtBearer()` för att validera tokens från Logto.
* Authority (Logto Issuer URL) och Audience MÅSTE kunna konfigureras via `appsettings.json` (t.ex. under sektionen `Logto`).
* Säkerställ att MCP-routen (`/sse`) kräver autentisering, exempelvis genom att applicera `.RequireAuthorization()` på endpointen, eller ställa in en global auktoriseringspolicy.

### 3. API-integration (Proxy)
* MCP-servern agerar proxy mot ett externt, befintligt backend-API.
* Backend-API:et har endast en endpoint som ska användas: `/search?query={query}`.
* Bas-URL:en till det befintliga API:et MÅSTE kunna injiceras via `appsettings.json` eller miljövariabler (t.ex. `RulesApi:BaseUrl`).
* Använd `IHttpClientFactory` för att hantera anropen till backend-API:et effektivt.

### 4. MCP Verktyg (Tools)
Du ska registrera ETT verktyg på MCP-servern med följande specifikation:
* **Namn:** `search_dod_rules`
* **Beskrivning:** "Söker i regelverket för Drakar och Demoner. Används för att hitta regler, stats för monster, magi eller utrustning."
* **Input Schema:** En parameter `query` av typen string.
* **Beteende:** 
  1. Extrahera söktermen (`query`).
  2. Bygg URL:en och gör ett `GET`-anrop till det underliggande API:et (via den konfigurerade HttpClienten).
  3. Om anropet lyckas (HTTP 200), returnera det råa JSON-svaret inkapslat som text i enlighet med MCP-standarden.
  4. Implementera robust felhantering. Om anropet misslyckas, returnera ett tydligt felmeddelande i verktygets svar så att LLM:en förstår vad som gick fel. Krascha inte servern.

### 5. Docker och Hosting
* Skapa en optimerad `Dockerfile` för en ASP.NET Core-applikation (använd officiella .NET SDK- och ASP.NET-images från Microsoft).
* Se till att applikationen lyssnar på standardportar för webb (exempelvis port 8080 i containern).

## Filer som ska genereras
Vänligen skapa följande struktur och filer:
1. `DodMcpServer.csproj` (med nödvändiga NuGet-paket: `ModelContextProtocol.AspNetCore`, `Microsoft.AspNetCore.Authentication.JwtBearer`).
2. `Program.cs` (Innehåller DI-uppsättning, Logto JWT-konfiguration, MCP-logik, HttpClient-konfiguration och säkrade Minimal API-routes).
3. `appsettings.json` (Med konfigurationsmall för API-bas-URL samt Logto `Authority` och `Audience`).
4. `Dockerfile` (För att bygga och köra webbapplikationen).
5. `README.md` (Med instruktioner för hur man bygger docker-imagen, startar servern, samt hur klienter skickar in Authorization-headern med en Bearer-token från Logto).

Vänligen generera koden nu.