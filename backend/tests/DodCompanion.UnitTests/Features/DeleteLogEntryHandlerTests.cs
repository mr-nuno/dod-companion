using Ardalis.Result;
using DodCompanion.Application.Common.Interfaces;
using DodCompanion.Application.Features.LogEntries.DeleteLogEntry;
using DodCompanion.Domain.LogEntry;
using NSubstitute;
using Shouldly;

namespace DodCompanion.UnitTests.Features;

public class DeleteLogEntryHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private readonly IUserSession _userSession = Substitute.For<IUserSession>();
    private readonly ITimelineNotifier _notifier = Substitute.For<ITimelineNotifier>();

    private DeleteLogEntryCommand.Handler CreateHandler() => new(_db, _userSession, _notifier);

    private static LogEntryAggregate ExistingEntry(string sessionId, string playerName) =>
        LogEntryAggregate.Create(sessionId, playerName, "Title", "body", "cave", DateTimeOffset.UtcNow, []);

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

        var result = await CreateHandler().Handle(new DeleteLogEntryCommand("LogEntries/1"), CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Unauthorized);
        _db.DidNotReceive().Delete(Arg.Any<LogEntryAggregate>());
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_EntryMissing()
    {
        AuthenticatedAs("sessions/1", "Aragorn");
        _db.LoadAsync<LogEntryAggregate>("LogEntries/1", Arg.Any<CancellationToken>()).Returns((LogEntryAggregate?)null);

        var result = await CreateHandler().Handle(new DeleteLogEntryCommand("LogEntries/1"), CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.NotFound);
        _db.DidNotReceive().Delete(Arg.Any<LogEntryAggregate>());
    }

    [Fact]
    public async Task Handle_Should_ReturnForbidden_When_NotOwnEntry()
    {
        AuthenticatedAs("sessions/1", "Aragorn");
        _db.LoadAsync<LogEntryAggregate>("LogEntries/1", Arg.Any<CancellationToken>())
            .Returns(ExistingEntry("sessions/1", "Legolas"));

        var result = await CreateHandler().Handle(new DeleteLogEntryCommand("LogEntries/1"), CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Forbidden);
        _db.DidNotReceive().Delete(Arg.Any<LogEntryAggregate>());
        await _notifier.DidNotReceive().LogEntryDeletedAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_DeleteAndNotify_When_OwnEntry()
    {
        AuthenticatedAs("sessions/1", "Aragorn");
        var entry = ExistingEntry("sessions/1", "Aragorn");
        _db.LoadAsync<LogEntryAggregate>("LogEntries/1", Arg.Any<CancellationToken>()).Returns(entry);

        var result = await CreateHandler().Handle(new DeleteLogEntryCommand("LogEntries/1"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        _db.Received(1).Delete(entry);
        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _notifier.Received(1).LogEntryDeletedAsync("sessions/1", "LogEntries/1", Arg.Any<CancellationToken>());
    }
}
