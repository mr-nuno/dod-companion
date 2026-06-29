using DodCompanion.Domain.Session;
using Shouldly;

namespace DodCompanion.UnitTests.Domain;

public class SessionAggregateTests
{
    private static PlayerInfo Player(string name, int kp = 10, int upf = 10, int fdt = 10) =>
        new(name, kp, upf, fdt);

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

        session.Join(Player("Aragorn"));

        session.Players.ShouldContain(p => p.Name == "Aragorn");
    }

    [Fact]
    public void Join_Should_BeIdempotent_When_SamePlayerJoinsTwice()
    {
        var session = SessionAggregate.Create("DRAGON", "join-token-123", DateTimeOffset.UtcNow);

        session.Join(Player("Aragorn", kp: 12));
        session.Join(Player("aragorn", kp: 15));

        session.Players.Count.ShouldBe(1);
        session.Players[0].Kp.ShouldBe(15);
    }
}
