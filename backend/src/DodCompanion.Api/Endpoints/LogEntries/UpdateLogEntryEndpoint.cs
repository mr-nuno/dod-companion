using DodCompanion.Application.Common.Dtos;
using DodCompanion.Application.Common.Models;
using DodCompanion.Application.Features.LogEntries.UpdateLogEntry;
using FastEndpoints;
using FluentValidation;
using MediatR;

namespace DodCompanion.Api.Endpoints.LogEntries;

/// <summary>HTTP request body for updating a log entry. The id comes from the route.</summary>
public sealed record UpdateLogEntryRequest(string Id, string? Title, string Content, List<string>? Tags);

/// <summary>PUT /log-entries/{id} — revises the author's own timeline entry and broadcasts the change.</summary>
public sealed class UpdateLogEntryEndpoint(ISender sender)
    : Endpoint<UpdateLogEntryRequest, ApiResponse<LogEntryDto>>
{
    public override void Configure()
    {
        Put("/log-entries/{id}");
        Summary(s =>
        {
            s.Summary = "Update one of the current player's own log entries.";
            s.ExampleRequest = new UpdateLogEntryRequest("LogEntries/1-A", "A glittering hoard", "The party found a chest with 300 gold.", ["Loot"]);
        });
        Tags("LogEntries");
    }

    public override async Task HandleAsync(UpdateLogEntryRequest req, CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateLogEntryCommand(req.Id, req.Title ?? string.Empty, req.Content, req.Tags ?? []), ct);
        await Send.ResponseAsync(result.ToApiResponse(), result.ToHttpStatusCode(), ct);
    }
}

/// <summary>HTTP-level validation for the update-log-entry request record.</summary>
public sealed class UpdateLogEntryRequestValidator : Validator<UpdateLogEntryRequest>
{
    public UpdateLogEntryRequestValidator()
    {
        RuleFor(x => x.Content).NotEmpty().WithMessage("Log entry content is required.");
    }
}
