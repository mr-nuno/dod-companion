namespace DodCompanion.Application.Common.Models;

/// <summary>
/// Standard response envelope returned by every endpoint. Matches the external Search API contract:
/// <c>{ success, data, error, validationErrors }</c>.
/// </summary>
public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Error { get; init; }
    public IReadOnlyCollection<ValidationError>? ValidationErrors { get; init; }

    public static ApiResponse<T> Ok(T data) => new() { Success = true, Data = data };

    public static ApiResponse<T> Fail(string error) => new() { Success = false, Error = error };

    public static ApiResponse<T> Invalid(IReadOnlyCollection<ValidationError> errors) =>
        new() { Success = false, ValidationErrors = errors };
}
