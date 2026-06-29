namespace DodCompanion.Application.Common.Dtos;

public sealed record SessionSummaryDto(
    string Id,
    string SessionId,
    string RoomCode,
    string Content,
    int EntryCount,
    DateTimeOffset GeneratedAt);
