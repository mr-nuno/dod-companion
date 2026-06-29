namespace DodCompanion.Application.Common.Interfaces;

/// <summary>Abstraction over the system clock. Never call <c>DateTimeOffset.UtcNow</c> directly.</summary>
public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
