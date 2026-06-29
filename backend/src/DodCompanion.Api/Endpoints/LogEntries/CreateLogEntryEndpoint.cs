using DodCompanion.Application.Common.Dtos;
using DodCompanion.Application.Common.Models;
using DodCompanion.Application.Features.LogEntries.CreateLogEntry;
using FastEndpoints;
using FluentValidation;
using MediatR;

namespace DodCompanion.Api.Endpoints.LogEntries;

/// <summary>HTTP request body for creating a log entry.</summary>
public sealed record CreateLogEntryRequest(string Content);

/// <summary>POST /log-entries — adds a timeline entry for the current session and broadcasts it.</summary>
public sealed class CreateLogEntryEndpoint(ISender sender)
    : Endpoint<CreateLogEntryRequest, ApiResponse<LogEntryDto>>
{
    public override void Configure()
    {
        Post("/log-entries");
        Summary(s =>
        {
            s.Summary = "Log a timeline event for the current session.";
            s.ExampleRequest = new CreateLogEntryRequest("The party found a chest with 200 gold.");
        });
        Tags("LogEntries");
    }

    public override async Task HandleAsync(CreateLogEntryRequest req, CancellationToken ct)
    {
        var result = await sender.Send(new CreateLogEntryCommand(req.Content), ct);
        await Send.ResponseAsync(result.ToApiResponse(), result.ToHttpStatusCode(), ct);
    }
}

/// <summary>HTTP-level validation for the create-log-entry request record.</summary>
public sealed class CreateLogEntryRequestValidator : Validator<CreateLogEntryRequest>
{
    public CreateLogEntryRequestValidator()
    {
        RuleFor(x => x.Content).NotEmpty().WithMessage("Log entry content is required.");
    }
}
