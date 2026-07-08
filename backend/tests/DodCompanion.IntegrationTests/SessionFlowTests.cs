using System.Net;
using System.Net.Http.Json;
using DodCompanion.Application.Common.Dtos;
using DodCompanion.Application.Common.Models;
using DodCompanion.Application.Features.LogEntries.GetTimeline;
using Shouldly;

namespace DodCompanion.IntegrationTests;

public class SessionFlowTests(DodCompanionApiFactory factory) : IClassFixture<DodCompanionApiFactory>
{
    private HttpClient CreateClient()
    {
        Skip.IfNot(DodCompanionApiFactory.RavenIsReachable(),
            $"RavenDB not reachable at {DodCompanionApiFactory.RavenUrl}; skipping integration test.");

        return factory.CreateClient();
    }

    // Provisions a room via the magic-link flow on a throwaway SL client (request → capture link → consume).
    // The room is created and the SL is added as a DM player; the returned body carries the join token.
    private async Task<CreatedRoomBody> CreateRoomAsync(string roomName)
    {
        var slClient = CreateClient();

        var request = await slClient.PostAsJsonAsync("/sessions/request-create",
            new RequestCreateBody(DodCompanionApiFactory.AllowedEmail, roomName));
        request.StatusCode.ShouldBe(HttpStatusCode.OK);

        var token = factory.Emails.DequeueToken();

        var consume = await slClient.PostAsJsonAsync("/sessions/consume-create", new ConsumeCreateBody(token));
        consume.StatusCode.ShouldBe(HttpStatusCode.OK);

        var created = await consume.Content.ReadFromJsonAsync<ApiResponse<SessionBody>>();
        created!.Success.ShouldBeTrue();
        var s = created.Data!;
        return new CreatedRoomBody(s.SessionId, s.RoomCode, s.JoinToken);
    }

    // Creates a room and joins it as the named player on the given client, signing that client in via cookie.
    private async Task<SessionBody> EnterRoomAsync(HttpClient client, string roomName, string playerName)
    {
        var room = await CreateRoomAsync(roomName);

        var joinResponse = await client.PostAsJsonAsync("/sessions/join",
            new JoinSessionRequestBody(room.JoinToken, playerName));
        joinResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var joined = await joinResponse.Content.ReadFromJsonAsync<ApiResponse<SessionBody>>();
        joined!.Success.ShouldBeTrue();
        return joined.Data!;
    }

    [SkippableFact]
    public async Task Create_Then_Join_Then_Me_Should_ReturnSamePlayerViaCookie()
    {
        var client = CreateClient();

        var session = await EnterRoomAsync(client, "integration", "Frodo");
        session.PlayerName.ShouldBe("Frodo");
        session.RoomCode.ShouldBe("INTEGRATION");
        session.JoinToken.ShouldNotBeNullOrWhiteSpace();

        var me = await client.GetFromJsonAsync<ApiResponse<SessionBody>>("/sessions/me");
        me!.Success.ShouldBeTrue();
        me.Data!.PlayerName.ShouldBe("Frodo");
        me.Data.SessionId.ShouldBe(session.SessionId);
        me.Data.JoinToken.ShouldBe(session.JoinToken);
    }

    [SkippableFact]
    public async Task ConsumeCreateLink_Should_SignInAsSL()
    {
        var client = CreateClient();

        var request = await client.PostAsJsonAsync("/sessions/request-create",
            new RequestCreateBody(DodCompanionApiFactory.AllowedEmail, "sl-room"));
        request.StatusCode.ShouldBe(HttpStatusCode.OK);

        var token = factory.Emails.DequeueToken();
        var consume = await client.PostAsJsonAsync("/sessions/consume-create", new ConsumeCreateBody(token));
        consume.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Consuming the link creates the room and signs the clicker in as the Game Master (SL).
        var me = await client.GetFromJsonAsync<ApiResponse<SessionBody>>("/sessions/me");
        me!.Success.ShouldBeTrue();
        me.Data!.PlayerName.ShouldBe("SL");
        me.Data.RoomCode.ShouldBe("SL-ROOM");
    }

    [SkippableFact]
    public async Task RequestCreate_Should_Fail_When_EmailNotAllowed()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/sessions/request-create",
            new RequestCreateBody("intruder@example.com", "locked-room"));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [SkippableFact]
    public async Task ConsumeCreateLink_Should_Fail_When_TokenReused()
    {
        var client = CreateClient();

        var request = await client.PostAsJsonAsync("/sessions/request-create",
            new RequestCreateBody(DodCompanionApiFactory.AllowedEmail, "once-room"));
        request.StatusCode.ShouldBe(HttpStatusCode.OK);

        var token = factory.Emails.DequeueToken();
        (await client.PostAsJsonAsync("/sessions/consume-create", new ConsumeCreateBody(token))).StatusCode.ShouldBe(HttpStatusCode.OK);

        // A magic link is single-use.
        var reuse = await CreateClient().PostAsJsonAsync("/sessions/consume-create", new ConsumeCreateBody(token));
        reuse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [SkippableFact]
    public async Task Join_Should_AddSecondPlayer_To_SameSession_ViaToken()
    {
        var room = await CreateRoomAsync("fellowship");

        var joinerClient = CreateClient();
        var joinResponse = await joinerClient.PostAsJsonAsync("/sessions/join",
            new JoinSessionRequestBody(room.JoinToken, "Sam"));
        joinResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var joined = await joinResponse.Content.ReadFromJsonAsync<ApiResponse<SessionBody>>();
        joined!.Success.ShouldBeTrue();
        joined.Data!.PlayerName.ShouldBe("Sam");
        joined.Data.SessionId.ShouldBe(room.SessionId);
    }

    [SkippableFact]
    public async Task GetPlayers_Should_ReturnAllJoinedPlayers_AfterReloadFromStore()
    {
        // Reproduces the F5 scenario: the roster must survive a fresh load from RavenDB,
        // not just live in SignalR-broadcast frontend state.
        var hostClient = CreateClient();
        var hostSession = await EnterRoomAsync(hostClient, "roster-room", "Frodo");

        var joinerClient = CreateClient();
        var joinResponse = await joinerClient.PostAsJsonAsync("/sessions/join",
            new JoinSessionRequestBody(hostSession.JoinToken, "Sam"));
        joinResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var players = await hostClient.GetFromJsonAsync<ApiResponse<PlayersBody>>("/sessions/players");
        players!.Success.ShouldBeTrue();
        // Roster carries the Game Master (SL, added on room creation) plus both joined players.
        players.Data!.Players.Count.ShouldBe(3);
        players.Data.Players.ShouldContain(p => p.Name == "SL");
        players.Data.Players.ShouldContain(p => p.Name == "Frodo");
        players.Data.Players.ShouldContain(p => p.Name == "Sam");
    }

    [SkippableFact]
    public async Task Join_Should_Return404_When_TokenUnknown()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/sessions/join",
            new JoinSessionRequestBody("not-a-real-token", "Gollum"));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [SkippableFact]
    public async Task Me_Should_Return401_When_NoCookie()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/sessions/me");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [SkippableFact]
    public async Task CreateLogEntry_Then_Timeline_Should_ContainEntry()
    {
        var client = CreateClient();
        await EnterRoomAsync(client, "timeline-room", "Sam");

        var create = await client.PostAsJsonAsync("/log-entries", new CreateLogEntryBody("Found the ring"));
        create.StatusCode.ShouldBe(HttpStatusCode.OK);

        var timeline = await client.GetFromJsonAsync<ApiResponse<TimelineResponse>>("/log-entries");
        timeline!.Success.ShouldBeTrue();
        timeline.Data!.Entries.ShouldContain(e => e.Content == "Found the ring" && e.PlayerName == "Sam");
    }

    [SkippableFact]
    public async Task RuleSearch_Should_ReturnStubbedMarkdownResult()
    {
        var client = CreateClient();
        await EnterRoomAsync(client, "rules-room", "Gandalf");

        var result = await client.GetFromJsonAsync<ApiResponse<RuleSearchResult>>("/rules/search?query=combat");

        result!.Success.ShouldBeTrue();
        result.Data!.Results.ShouldHaveSingleItem().Content.ShouldBe("**Roll initiative**");
    }

    [SkippableFact]
    public async Task CreateLogEntry_Should_Return400_When_ContentEmpty()
    {
        var client = CreateClient();
        await EnterRoomAsync(client, "validation-room", "Pippin");

        var response = await client.PostAsJsonAsync("/log-entries", new CreateLogEntryBody(""));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [SkippableFact]
    public async Task GenerateSummary_Should_ExcludeInfoTaggedEntries()
    {
        var client = CreateClient();
        await EnterRoomAsync(client, "summary-room", "Gimli");

        var create1 = await client.PostAsJsonAsync("/log-entries", new CreateLogEntryBody("Fought an orc", ["Strid"]));
        create1.StatusCode.ShouldBe(HttpStatusCode.OK);

        var create2 = await client.PostAsJsonAsync("/log-entries", new CreateLogEntryBody("Joined the chat", ["info"]));
        create2.StatusCode.ShouldBe(HttpStatusCode.OK);

        var generateResponse = await client.PostAsync("/sessions/summary", null);
        generateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var summaryEnvelope = await generateResponse.Content.ReadFromJsonAsync<ApiResponse<SessionSummaryDto>>();
        summaryEnvelope!.Success.ShouldBeTrue();
        var summary = summaryEnvelope.Data!;

        summary.EntryCount.ShouldBe(1);
        summary.Content.ShouldContain("Fought an orc");
        summary.Content.ShouldNotContain("Joined the chat");
        summary.Content.ShouldNotContain("info");
    }

    // Local request/response shapes (avoid coupling tests to API endpoint records).
    private sealed record RequestCreateBody(string Email, string RoomName);
    private sealed record ConsumeCreateBody(string Token);
    private sealed record JoinSessionRequestBody(string JoinToken, string PlayerName, int Kp = 10, int UpptackFara = 10, int FinnaDoldaTing = 10);
    private sealed record CreateLogEntryBody(string Content, List<string>? Tags = null);
    private sealed record CreatedRoomBody(string SessionId, string RoomCode, string JoinToken);
    private sealed record SessionBody(string SessionId, string RoomCode, string PlayerName, string JoinToken);
    private sealed record PlayersBody(List<PlayerBody> Players);
    private sealed record PlayerBody(string Name, int Kp, int UpptackFara, int FinnaDoldaTing);
}
