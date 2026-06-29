using System.Net.Http.Headers;

namespace DodCompanion.Infrastructure.Search;

/// <summary>Attaches the (cached) client-credentials bearer token to every outgoing Rules API request.</summary>
public sealed class RulesAuthHandler(IRulesTokenProvider tokenProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await tokenProvider.GetAccessTokenAsync(cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }
}
