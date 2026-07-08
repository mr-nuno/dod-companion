namespace DodCompanion.Application.Common.Interfaces;

/// <summary>
/// Sends transactional email. Implemented in Infrastructure (Resend) behind this seam so handlers
/// stay free of any provider/transport detail and remain unit-testable.
/// </summary>
public interface IEmailSender
{
    /// <summary>Send a single HTML email to one recipient.</summary>
    Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct);
}
