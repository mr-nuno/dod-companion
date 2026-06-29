using Ardalis.Result;
using DodCompanion.Application.Common.Dtos;
using DodCompanion.Application.Common.Interfaces;
using DodCompanion.Application.Features.LogEntries.CreateLogEntry;
using DodCompanion.Domain.LogEntry;
using NSubstitute;
using Shouldly;

namespace DodCompanion.UnitTests.Features;

public class CreateLogEntryHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private readonly IUserSession _userSession = Substitute.For<IUserSession>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly ITimelineNotifier _notifier = Substitute.For<ITimelineNotifier>();

    private CreateLogEntryCommand.Handler CreateHandler() => new(_db, _userSession, _clock, _notifier);

    [Fact]
    public async Task Handle_Should_ReturnUnauthorized_When_NotAuthenticated()
    {
        _userSession.IsAuthenticated.Returns(false);

        var result = await CreateHandler().Handle(new CreateLogEntryCommand("hello"), CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Unauthorized);
        await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_PersistTrimmedEntryAndNotify_When_Authenticated()
    {
        var now = DateTimeOffset.UtcNow;
        _userSession.IsAuthenticated.Returns(true);
        _userSession.SessionId.Returns("sessions/1");
        _userSession.PlayerName.Returns("Aragorn");
        _clock.UtcNow.Returns(now);

        var result = await CreateHandler().Handle(new CreateLogEntryCommand("  found gold  "), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Content.ShouldBe("found gold");
        result.Value.PlayerName.ShouldBe("Aragorn");
        result.Value.SessionId.ShouldBe("sessions/1");
        result.Value.Timestamp.ShouldBe(now);

        await _db.Received(1).StoreAsync(Arg.Any<LogEntryAggregate>(), Arg.Any<CancellationToken>());
        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _notifier.Received(1).LogEntryCreatedAsync("sessions/1", Arg.Any<LogEntryDto>(), Arg.Any<CancellationToken>());
    }
}
