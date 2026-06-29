using Ardalis.Result;
using FluentValidation;
using MediatR;

namespace DodCompanion.Application.Common.Behaviors;

/// <summary>
/// Runs FluentValidation for every request before its handler. On failure, short-circuits with an
/// invalid <see cref="Result"/> (mapped to HTTP 400) for all callers — HTTP and in-process alike.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Ardalis.Result.IResult
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, ct)));
        var failures = results.SelectMany(r => r.Errors).Where(f => f is not null).ToList();

        if (failures.Count == 0)
        {
            return await next();
        }

        var errors = failures
            .Select(f => new ValidationError { Identifier = f.PropertyName, ErrorMessage = f.ErrorMessage })
            .ToArray();

        return CreateInvalidResult(errors);
    }

    // Result and Result<T> both expose a static Invalid(params ValidationError[]) returning their own type.
    private static TResponse CreateInvalidResult(ValidationError[] errors)
    {
        var method = typeof(TResponse).GetMethod(nameof(Result.Invalid), [typeof(ValidationError[])])
            ?? throw new InvalidOperationException($"{typeof(TResponse)} has no static Invalid(ValidationError[]) factory.");

        return (TResponse)method.Invoke(null, [errors])!;
    }
}
