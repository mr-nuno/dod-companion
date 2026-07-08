using System.Net.Http.Json;
using DodCompanion.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DodCompanion.Infrastructure.Email;

/// <summary>
/// <see cref="IEmailSender"/> over the Resend HTTP API. When no API key is configured it falls back to
/// logging the message (including its body) instead of sending — so local development works without Resend.
/// </summary>
public sealed class ResendEmailSender(HttpClient http, IOptions<ResendOptions> options, ILogger<ResendEmailSender> logger)
    : IEmailSender
{
    private readonly ResendOptions _options = options.Value;

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            // Log-only mode: no key configured. Emit the body so a developer can copy the magic link.
            logger.LogWarning(
                "Resend API key not configured — email to {To} NOT sent. Subject: {Subject}\n{Body}",
                toEmail, subject, htmlBody);
            return;
        }

        var payload = new ResendEmailPayload(
            From: $"{_options.FromName} <{_options.FromEmail}>",
            To: [toEmail],
            Subject: subject,
            Html: htmlBody);

        using var response = await http.PostAsJsonAsync("/emails", payload, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            logger.LogError("Resend send failed with {Status}: {Body}", (int)response.StatusCode, body);
            throw new InvalidOperationException($"Email delivery failed ({(int)response.StatusCode}).");
        }
    }

    private sealed record ResendEmailPayload(string From, string[] To, string Subject, string Html);
}
