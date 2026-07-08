using Ardalis.Result;
using DodCompanion.Application.Common.Dtos;
using DodCompanion.Application.Common.Interfaces;
using DodCompanion.Application.Features.LogEntries.UpdateLogEntry;
using DodCompanion.Domain.LogEntry;
using NSubstitute;
using Shouldly;

namespace DodCompanion.UnitTests.Features;

public class UpdateLogEntryHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private readonly IUserSession _userSession = Substitute.For<IUserSession>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly ITimelineNotifier _notifier = Substitute.For<ITimelineNotifier>();

    private UpdateLogEntryCommand.Handler CreateHandler() => new(_db, _userSession, _clock, _notifier);

    private static LogEntryAggregate ExistingEntry(string sessionId, string playerName) =>
        LogEntryAggregate.Create(sessionId, playerName, "Old title", "old body", "ruins", DateTimeOffset.UtcNow, ["Loot"]);

    private void AuthenticatedAs(string sessionId, string playerName)
    {
        _userSession.IsAuthenticated.Returns(true);
        _userSession.SessionId.Returns(sessionId);
        _userSession.PlayerName.Returns(playerName);
    }

    [Fact]
    public async Task Handle_Should_ReturnUnauthorized_When_NotAuthenticated()
    {
        _userSession.IsAuthenticated.Returns(false);

        var result = await CreateHandler().Handle(new UpdateLogEntryCommand("LogEntries/1", "t", "c", []), CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Unauthorized);
        await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_EntryMissing()
    {
        AuthenticatedAs("sessions/1", "Aragorn");
        _db.LoadAsync<LogEntryAggregate>("LogEntries/1", Arg.Any<CancellationToken>()).Returns((LogEntryAggregate?)null);

        var result = await CreateHandler().Handle(new UpdateLogEntryCommand("LogEntries/1", "t", "c", []), CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.NotFound);
        await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnForbidden_When_NotOwnEntry()
    {
        AuthenticatedAs("sessions/1", "Aragorn");
        _db.LoadAsync<LogEntryAggregate>("LogEntries/1", Arg.Any<CancellationToken>())
            .Returns(ExistingEntry("sessions/1", "Legolas"));

        var result = await CreateHandler().Handle(new UpdateLogEntryCommand("LogEntries/1", "t", "c", []), CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Forbidden);
        await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _notifier.DidNotReceive().LogEntryUpdatedAsync(Arg.Any<string>(), Arg.Any<LogEntryDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_EditAndNotify_When_OwnEntry()
    {
        var now = DateTimeOffset.UtcNow;
        AuthenticatedAs("sessions/1", "Aragorn");
        _clock.UtcNow.Returns(now);
        var entry = ExistingEntry("sessions/1", "Aragorn");
        _db.LoadAsync<LogEntryAggregate>("LogEntries/1", Arg.Any<CancellationToken>()).Returns(entry);

        var result = await CreateHandler().Handle(
            new UpdateLogEntryCommand("LogEntries/1", "  New title  ", "  new body  ", ["Strid"]), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Title.ShouldBe("New title");
        result.Value.Content.ShouldBe("new body");
        result.Value.Tags.ShouldBe(["Strid"]);
        result.Value.HeroImage.ShouldBe("ruins"); // preserved
        result.Value.UpdatedAt.ShouldBe(now);

        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _notifier.Received(1).LogEntryUpdatedAsync("sessions/1", Arg.Any<LogEntryDto>(), Arg.Any<CancellationToken>());
    }
}
