using DodCompanion.Application.Common.Dtos;
using DodCompanion.Application.Common.Models;
using DodCompanion.Application.Features.SessionSummary;
using FastEndpoints;
using MediatR;

namespace DodCompanion.Api.Endpoints.Sessions;

/// <summary>POST /sessions/summary — generates and persists a markdown summary of the session timeline.</summary>
public sealed class GenerateSummaryEndpoint(ISender sender)
    : EndpointWithoutRequest<ApiResponse<SessionSummaryDto>>
{
    public override void Configure()
    {
        Post("/sessions/summary");
        Summary(s => s.Summary = "Generate and persist a markdown summary of the session timeline.");
        Tags("Sessions");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await sender.Send(new GenerateSummaryCommand(), ct);
        await Send.ResponseAsync(result.ToApiResponse(), result.ToHttpStatusCode(), ct);
    }
}

/// <summary>GET /sessions/summary — returns the stored summary for the current session.</summary>
public sealed class GetSummaryEndpoint(ISender sender)
    : EndpointWithoutRequest<ApiResponse<SessionSummaryDto>>
{
    public override void Configure()
    {
        Get("/sessions/summary");
        Summary(s => s.Summary = "Get the stored session summary, if one has been generated.");
        Tags("Sessions");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await sender.Send(new GetSummaryQuery(), ct);
        await Send.ResponseAsync(result.ToApiResponse(), result.ToHttpStatusCode(), ct);
    }
}
