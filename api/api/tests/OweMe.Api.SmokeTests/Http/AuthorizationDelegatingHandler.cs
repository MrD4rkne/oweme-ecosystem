using System.Net.Http.Headers;

namespace OweMe.Api.SmokeTests.Http;

internal sealed class AuthorizationDelegatingHandler(ITokenManager tokenManager) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var accessToken = await GetAccessTokenAsync(cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        return await tokenManager.GetAccessTokenAsync(cancellationToken);
    }
}