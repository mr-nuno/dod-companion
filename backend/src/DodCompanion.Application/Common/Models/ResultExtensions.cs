using Ardalis.Result;

namespace DodCompanion.Application.Common.Models;

/// <summary>
/// Maps <see cref="Ardalis.Result.Result{T}"/> to the <see cref="ApiResponse{T}"/> envelope and HTTP status.
/// Endpoints use these — never inline the switch.
/// </summary>
public static class ResultExtensions
{
    public static ApiResponse<T> ToApiResponse<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return ApiResponse<T>.Ok(result.Value);
        }

        if (result.Status == ResultStatus.Invalid)
        {
            // Fully qualify our ValidationError to avoid CS0104 ambiguity with Ardalis.Result.ValidationError.
            var errors = result.ValidationErrors
                .Select(e => new Models.ValidationError(e.Identifier, e.ErrorMessage))
                .ToList();
            return ApiResponse<T>.Invalid(errors);
        }

        var message = result.Errors.FirstOrDefault() ?? DefaultMessage(result.Status);
        return ApiResponse<T>.Fail(message);
    }

    public static int ToHttpStatusCode<T>(this Result<T> result) => result.Status switch
    {
        ResultStatus.Ok => StatusCodes.Status200OK,
        ResultStatus.NotFound => StatusCodes.Status404NotFound,
        ResultStatus.Invalid => StatusCodes.Status400BadRequest,
        ResultStatus.Conflict => StatusCodes.Status409Conflict,
        ResultStatus.Unauthorized => StatusCodes.Status401Unauthorized,
        ResultStatus.Forbidden => StatusCodes.Status403Forbidden,
        _ => StatusCodes.Status400BadRequest,
    };

    private static string DefaultMessage(ResultStatus status) => status switch
    {
        ResultStatus.NotFound => "The requested resource was not found.",
        ResultStatus.Conflict => "The request conflicts with the current state.",
        ResultStatus.Unauthorized => "Authentication is required.",
        ResultStatus.Forbidden => "You are not allowed to perform this action.",
        _ => "The request could not be completed.",
    };

    // Local mirror of the ASP.NET status code constants so the Application layer needs no ASP.NET dependency.
    private static class StatusCodes
    {
        public const int Status200OK = 200;
        public const int Status400BadRequest = 400;
        public const int Status401Unauthorized = 401;
        public const int Status403Forbidden = 403;
        public const int Status404NotFound = 404;
        public const int Status409Conflict = 409;
    }
}
