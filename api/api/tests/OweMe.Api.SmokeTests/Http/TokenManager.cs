using Duende.IdentityModel.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OweMe.Api.SmokeTests.Configuration;

namespace OweMe.Api.SmokeTests.Http;

internal sealed class TokenManager(
    HttpClient client,
    IMemoryCache memoryCache,
    IDiscoveryCache discoveryCache,
    IOptions<IdentityProviderSettings> identityProviderSettings,
    IOptions<UserSettings> userSettings) : ITokenManager
{
    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        var key = CreateKeyFor(userSettings.Value.Username, userSettings.Value.Scope);
        if (memoryCache.TryGetValue(key, out string? accessToken))
        {
            return accessToken!;
        }

        var discoveryDocument = await discoveryCache.GetAsync();
        if (discoveryDocument.IsError)
        {
            throw new FailedToRetrieveDiscoveryDocumentException(identityProviderSettings.Value.Address,
                discoveryDocument.Error);
        }

        var tokenResponse = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
        {
            Address = discoveryDocument.TokenEndpoint,
            ClientId = identityProviderSettings.Value.ClientId,
            ClientSecret = identityProviderSettings.Value.ClientSecret,
            UserName = userSettings.Value.Username,
            Password = userSettings.Value.Password,
        }, cancellationToken);
        if (tokenResponse.IsError)
        {
            throw new FailedToObtainTokenException(userSettings.Value.Username, userSettings.Value.Scope,
                tokenResponse.Error);
        }

        accessToken = tokenResponse.AccessToken!;

        var cacheTime = TimeSpan.FromSeconds(tokenResponse.ExpiresIn * 0.8);
        memoryCache.Set(key, accessToken, cacheTime);

        return accessToken;
    }

    private static string CreateKeyFor(string username, string scope)
    {
        return $"{username}:{scope}";
    }

    private sealed class FailedToRetrieveDiscoveryDocumentException(string url, string? error)
        : Exception($"Failed to retrieve discovery document from {url}: {error}");

    private sealed class FailedToObtainTokenException(string user, string scope, string? error)
        : Exception($"Failed to obtain token for user '{user}' with scope '{scope}': {error}");
}