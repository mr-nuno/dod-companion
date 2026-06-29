using DodCompanion.Domain.Session;
using Shouldly;

namespace DodCompanion.UnitTests.Domain;

public class SessionAggregateTests
{
    [Fact]
    public void Create_Should_SetRoomCodeTokenAndCreatedAt_When_Constructed()
    {
        var now = DateTimeOffset.UtcNow;

        var session = SessionAggregate.Create("DRAGON", "join-token-123", now);

        session.RoomCode.ShouldBe("DRAGON");
        session.JoinToken.ShouldBe("join-token-123");
        session.CreatedAt.ShouldBe(now);
        session.Players.ShouldBeEmpty();
    }

    [Fact]
    public void Join_Should_AddPlayer_When_New()
    {
        var session = SessionAggregate.Create("DRAGON", "join-token-123", DateTimeOffset.UtcNow);

        session.Join("Aragorn");

        session.Players.ShouldContain("Aragorn");
    }

    [Fact]
    public void Join_Should_BeIdempotent_When_SamePlayerJoinsTwice()
    {
        var session = SessionAggregate.Create("DRAGON", "join-token-123", DateTimeOffset.UtcNow);

        session.Join("Aragorn");
        session.Join("aragorn");

        session.Players.Count.ShouldBe(1);
    }
}
