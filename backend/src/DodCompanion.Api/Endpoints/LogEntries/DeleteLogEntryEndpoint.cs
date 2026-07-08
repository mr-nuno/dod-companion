using DodCompanion.Application.Common.Models;
using DodCompanion.Application.Features.LogEntries.DeleteLogEntry;
using FastEndpoints;
using MediatR;

namespace DodCompanion.Api.Endpoints.LogEntries;

/// <summary>DELETE /log-entries/{id} — removes the author's own timeline entry and broadcasts the removal.</summary>
public sealed class DeleteLogEntryEndpoint(ISender sender) : EndpointWithoutRequest<ApiResponse<bool>>
{
    public override void Configure()
    {
        Delete("/log-entries/{id}");
        Summary(s => s.Summary = "Delete one of the current player's own log entries.");
        Tags("LogEntries");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<string>("id") ?? string.Empty;
        var result = await sender.Send(new DeleteLogEntryCommand(id), ct);
        await Send.ResponseAsync(result.ToApiResponse(), result.ToHttpStatusCode(), ct);
    }
}
