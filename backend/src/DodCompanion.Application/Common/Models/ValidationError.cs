namespace DodCompanion.Application.Common.Models;

/// <summary>A single field-level validation failure surfaced in the <see cref="ApiResponse{T}"/> envelope.</summary>
public sealed record ValidationError(string Identifier, string ErrorMessage);
