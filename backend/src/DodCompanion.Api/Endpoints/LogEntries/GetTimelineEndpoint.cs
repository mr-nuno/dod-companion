using DodCompanion.Application.Common.Models;
using DodCompanion.Application.Features.LogEntries.GetTimeline;
using FastEndpoints;
using MediatR;

namespace DodCompanion.Api.Endpoints.LogEntries;

/// <summary>GET /log-entries — the chronological timeline for the current session (initial load).</summary>
public sealed class GetTimelineEndpoint(ISender sender) : EndpointWithoutRequest<ApiResponse<TimelineResponse>>
{
    public override void Configure()
    {
        Get("/log-entries");
        Summary(s => s.Summary = "Get the chronological timeline for the current session.");
        Tags("LogEntries");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await sender.Send(new GetTimelineQuery(), ct);
        await Send.ResponseAsync(result.ToApiResponse(), result.ToHttpStatusCode(), ct);
    }
}
