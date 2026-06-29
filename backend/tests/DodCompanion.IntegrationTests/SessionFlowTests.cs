using System.Net;
using System.Net.Http.Json;
using DodCompanion.Application.Common.Dtos;
using DodCompanion.Application.Common.Models;
using DodCompanion.Application.Features.LogEntries.GetTimeline;
using DodCompanion.Application.Features.Sessions.JoinSession;
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

    [SkippableFact]
    public async Task Join_Then_Me_Should_ReturnSamePlayerViaCookie()
    {
        var client = CreateClient();

        var joinResponse = await client.PostAsJsonAsync("/sessions/join",
            new JoinSessionRequestBody("integration", "Frodo"));
        joinResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var join = await joinResponse.Content.ReadFromJsonAsync<ApiResponse<JoinSessionResponse>>();
        join!.Success.ShouldBeTrue();
        join.Data!.PlayerName.ShouldBe("Frodo");
        join.Data.RoomCode.ShouldBe("INTEGRATION");

        var me = await client.GetFromJsonAsync<ApiResponse<MeBody>>("/sessions/me");
        me!.Success.ShouldBeTrue();
        me.Data!.PlayerName.ShouldBe("Frodo");
        me.Data.SessionId.ShouldBe(join.Data.SessionId);
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
        await client.PostAsJsonAsync("/sessions/join", new JoinSessionRequestBody("timeline-room", "Sam"));

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
        await client.PostAsJsonAsync("/sessions/join", new JoinSessionRequestBody("rules-room", "Gandalf"));

        var result = await client.GetFromJsonAsync<ApiResponse<RuleSearchResult>>("/rules/search?query=combat");

        result!.Success.ShouldBeTrue();
        result.Data!.Results.ShouldHaveSingleItem().Content.ShouldBe("**Roll initiative**");
    }

    [SkippableFact]
    public async Task CreateLogEntry_Should_Return400_When_ContentEmpty()
    {
        var client = CreateClient();
        await client.PostAsJsonAsync("/sessions/join", new JoinSessionRequestBody("validation-room", "Pippin"));

        var response = await client.PostAsJsonAsync("/log-entries", new CreateLogEntryBody(""));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    // Local request/response shapes (avoid coupling tests to API endpoint records).
    private sealed record JoinSessionRequestBody(string RoomCode, string PlayerName);
    private sealed record CreateLogEntryBody(string Content);
    private sealed record MeBody(string SessionId, string PlayerName);
}
