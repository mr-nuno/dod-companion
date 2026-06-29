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

    // Provisions a room with the configured host key. Creation alone does NOT authenticate the client.
    private static async Task<CreatedRoomBody> CreateRoomAsync(HttpClient client, string roomName)
    {
        var response = await client.PostAsJsonAsync("/sessions/create",
            new CreateSessionRequestBody(roomName, DodCompanionApiFactory.HostKey));
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var created = await response.Content.ReadFromJsonAsync<ApiResponse<CreatedRoomBody>>();
        created!.Success.ShouldBeTrue();
        return created.Data!;
    }

    // Creates a room and joins it as the named player, signing the client in via cookie.
    private static async Task<SessionBody> EnterRoomAsync(HttpClient client, string roomName, string playerName)
    {
        var room = await CreateRoomAsync(client, roomName);

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
    public async Task Create_Should_NotAuthenticate_Until_Joined()
    {
        var client = CreateClient();

        await CreateRoomAsync(client, "lobby-room");

        // Provisioning a room must not sign anyone in — a player only exists after joining via the token.
        var me = await client.GetAsync("/sessions/me");
        me.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [SkippableFact]
    public async Task Create_Should_Return403_When_HostKeyWrong()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/sessions/create",
            new CreateSessionRequestBody("locked-room", "wrong-key"));

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [SkippableFact]
    public async Task Join_Should_AddSecondPlayer_To_SameSession_ViaToken()
    {
        var hostClient = CreateClient();
        var room = await CreateRoomAsync(hostClient, "fellowship");

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

    // Local request/response shapes (avoid coupling tests to API endpoint records).
    private sealed record CreateSessionRequestBody(string RoomName, string HostKey);
    private sealed record JoinSessionRequestBody(string JoinToken, string PlayerName);
    private sealed record CreateLogEntryBody(string Content);
    private sealed record CreatedRoomBody(string SessionId, string RoomCode, string JoinToken);
    private sealed record SessionBody(string SessionId, string RoomCode, string PlayerName, string JoinToken);
}
