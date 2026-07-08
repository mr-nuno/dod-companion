using Ardalis.Result;
using DodCompanion.Application.Common.Configuration;
using DodCompanion.Application.Common.Interfaces;
using DodCompanion.Application.Features.Sessions.RequestCreateLink;
using DodCompanion.Domain.SessionLink;
using NSubstitute;
using Shouldly;

namespace DodCompanion.UnitTests.Features;

public class RequestCreateLinkHandlerTests
{
    private readonly IApplicationDbContext _db = Substitute.For<IApplicationDbContext>();
    private readonly ITokenGenerator _tokens = Substitute.For<ITokenGenerator>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IEmailSender _email = Substitute.For<IEmailSender>();

    private readonly SessionOptions _options = new()
    {
        AllowedDmEmails = ["sl@example.com"],
        AppBaseUrl = "https://companion.example",
        MagicLinkTtlMinutes = 15,
    };

    private RequestCreateLinkCommand.Handler CreateHandler() => new(_db, _tokens, _clock, _email, _options);

    [Fact]
    public async Task Handle_Should_Fail_When_EmailNotAllowed()
    {
        var result = await CreateHandler().Handle(new RequestCreateLinkCommand("intruder@example.com", "Crypt"), CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _email.DidNotReceive().SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_StoreLinkAndEmailIt_When_EmailAllowed()
    {
        var now = DateTimeOffset.UtcNow;
        _clock.UtcNow.Returns(now);
        _tokens.NewJoinToken().Returns("magic-token");

        var result = await CreateHandler().Handle(new RequestCreateLinkCommand("  SL@Example.com ", "  Crypt  "), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        await _db.Received(1).StoreAsync(
            Arg.Is<SessionLinkAggregate>(l => l.Token == "magic-token" && l.RoomName == "Crypt" && l.Email == "SL@Example.com"),
            Arg.Any<CancellationToken>());
        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _email.Received(1).SendAsync(
            "SL@Example.com",
            Arg.Any<string>(),
            Arg.Is<string>(html => html.Contains("https://companion.example/create?token=magic-token")),
            Arg.Any<CancellationToken>());
    }
}
