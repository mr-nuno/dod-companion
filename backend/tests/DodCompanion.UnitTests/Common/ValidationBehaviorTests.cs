using Ardalis.Result;
using DodCompanion.Application.Common.Behaviors;
using FluentValidation;
using Shouldly;

namespace DodCompanion.UnitTests.Common;

public class ValidationBehaviorTests
{
    private sealed record SampleRequest(string Name);

    private sealed class SampleValidator : AbstractValidator<SampleRequest>
    {
        public SampleValidator() =>
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
    }

    [Fact]
    public async Task Handle_Should_ReturnInvalid_And_NotCallNext_When_ValidationFails()
    {
        var nextCalled = false;
        var behavior = new ValidationBehavior<SampleRequest, Result<string>>([new SampleValidator()]);

        var result = await behavior.Handle(
            new SampleRequest(""),
            () =>
            {
                nextCalled = true;
                return Task.FromResult(Result.Success("ok"));
            },
            CancellationToken.None);

        result.Status.ShouldBe(ResultStatus.Invalid);
        result.ValidationErrors.ShouldContain(e => e.ErrorMessage == "Name is required.");
        nextCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_Should_CallNext_When_ValidationPasses()
    {
        var behavior = new ValidationBehavior<SampleRequest, Result<string>>([new SampleValidator()]);

        var result = await behavior.Handle(
            new SampleRequest("valid"),
            () => Task.FromResult(Result.Success("ok")),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("ok");
    }
}
