# Project: TTRPG Companion Application

## 1. System Overview
We are building a web application to be used during Tabletop RPG sessions. The application serves two primary purposes: querying an external rules API and maintaining a shared timeline of events logged by the players. 

The architecture must follow a "Confidential Client" pattern (Backend-for-Frontend / BFF). The frontend must never hold sensitive API keys. The .NET backend will act as the confidential client, handling authentication, data storage, and proxying requests to external services.

## 2. Tech Stack
* **Frontend:** React, TypeScript, Tailwind CSS. 
  *(Note: Apply the user's globally configured rules and preferences for React/TS/Tailwind).*
* **BFF:** .NET (C#). 
  *(Note: Apply the user's globally configured rules and preferences for .NET architecture, project structure, and coding standards).*
* **Database:** RavenDb

### 2.1 Search API
* **Pdf search api**  https://pdf-search-api-1-0-0.onrender.com
* **OAS specification** http://localhost:5000/openapi/v1.json 

## 3. Core Features & Requirements

### Feature 1: "Menti-style" Simple Authentication
* **Flow:** No complex user registration. Users enter the app via a simple "Room Code" or "Session PIN" combined with a "Player Name".
* **Implementation:** The backend issues a secure, HttpOnly authentication cookie or a simple JWT token upon successful entry. 
* **State:** The backend maintains basic session state so it knows which player in which session is making a request.

### Feature 2: Shared Timeline & Event Logging
* **Flow:** Any authenticated player can submit a log entry (e.g., text note, event, loot).
* **Storage:** These entries must be saved to RavenDB.
* **View:** The React UI must feature a chronological "Timeline View" where all players can see every event logged during the session. Implement real-time updates or simple polling to keep the timeline fresh for everyone.

### Feature 3: Rule Questions (API Proxy)
* **Flow:** Players can ask rule questions through the React UI via a search input.
* **Security & Proxy:** The backend must expose an endpoint for this. When the frontend calls this endpoint, the backend forwards the request to the external Rules API. The backend must append the required JWT Bearer token from its secure configuration (`appsettings.json` or user secrets).
* **External API Contract:** * **Target:** `GET {BaseUrl}/search?query={term}`
  * **Auth:** HTTP Header `Authorization: Bearer <token>`
  * **Response Format:**
    ```json
    {
      "success": true,
      "data": {
        "query": "search term",
        "totalHits": 1,
        "results": [
          {
            "sourceFileName": "rulebook.pdf",
            "physicalPageNumber": 42,
            "header": "Combat Rules",
            "content": "**Markdown formatted text here...**",
            "searchScore": 0.95
          }
        ]
      }
    }
    ```
* **Response & Rendering:** The .NET BFF should deserialize this response and proxy the relevant data (especially the `results` array) to the React frontend. The React application must securely parse and render the `content` Markdown string into HTML using a library like `react-markdown`.

## 4. Database Models (RavenDB)
Initial document models to implement:

* **SessionDocument**: 
  * `Id` (string)
  * `RoomCode` (string)
  * `CreatedAt` (DateTimeOffset)
* **LogEntryDocument**:
  * `Id` (string)
  * `SessionId` (string)
  * `PlayerName` (string)
  * `Content` (string)
  * `Timestamp` (DateTimeOffset)

## 5. Instructions for the AI Assistant
1. Initialize the project structure (separate folders for the React frontend and the .NET backend) adhering strictly to the user's global structure rules.
2. Set up the RavenDB client connection using dependency injection in the .NET project.
3. Scaffold the authentication endpoints (Login with Room Code).
4. Scaffold the Event Logging endpoints (Create Log, Get Timeline for Session).
5. Implement an `HttpClient` (preferably using `IHttpClientFactory` and Typed Clients) to communicate with the external Rules API, automatically attaching the JWT Bearer token.
6. Scaffold the BFF proxy endpoint that calls the external `/search` API and returns the data to the frontend.
7. Build the React frontend using TypeScript and Tailwind CSS. Implement a Markdown renderer to display the rule question responses from the proxy endpoint.
8. Ensure CORS, HttpOnly cookies (if used), and security headers are configured appropriately for the BFF architecture.
9. Strictly follow the user's globally defined .NET and React patterns when implementing logic.