using DodCompanion.Domain.Session;
using Shouldly;

namespace DodCompanion.UnitTests.Domain;

public class SessionAggregateTests
{
    [Fact]
    public void Create_Should_SetRoomCodeAndCreatedAt_When_Constructed()
    {
        var now = DateTimeOffset.UtcNow;

        var session = SessionAggregate.Create("DRAGON", now);

        session.RoomCode.ShouldBe("DRAGON");
        session.CreatedAt.ShouldBe(now);
        session.Players.ShouldBeEmpty();
    }

    [Fact]
    public void Join_Should_AddPlayer_When_New()
    {
        var session = SessionAggregate.Create("DRAGON", DateTimeOffset.UtcNow);

        session.Join("Aragorn");

        session.Players.ShouldContain("Aragorn");
    }

    [Fact]
    public void Join_Should_BeIdempotent_When_SamePlayerJoinsTwice()
    {
        var session = SessionAggregate.Create("DRAGON", DateTimeOffset.UtcNow);

        session.Join("Aragorn");
        session.Join("aragorn");

        session.Players.Count.ShouldBe(1);
    }
}
