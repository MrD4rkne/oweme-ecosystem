using System.Net.Http.Headers;

namespace OweMe.IntegrationTests.Authentication;

public static class HttpClientExtensions
{
    public static HttpClient AsUser(
        this HttpClient client,
        string userId,
        string name = "Test User",
        string scope = "oweme-api")
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.SchemeName);

        client.DefaultRequestHeaders.SetHeader(TestAuthHandler.UserIdHeader, userId);
        client.DefaultRequestHeaders.SetHeader(TestAuthHandler.UserEmailHeader, name);
        client.DefaultRequestHeaders.SetHeader(TestAuthHandler.ScopeHeader, scope);

        return client;
    }

    public static HttpClient AsUser(
        this HttpClient client,
        TestUser user)
    {
        return client.AsUser(user.Id.ToString(), user.Email);
    }

    private static void SetHeader(this HttpRequestHeaders headers, string name, string value)
    {
        if (headers.Contains(name)) headers.Remove(name);
        headers.Add(name, value);
    }

    public static HttpClient AsAnonymous(this HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
        client.DefaultRequestHeaders.Remove(TestAuthHandler.UserIdHeader);
        client.DefaultRequestHeaders.Remove(TestAuthHandler.UserEmailHeader);
        client.DefaultRequestHeaders.Remove(TestAuthHandler.ScopeHeader);
        return client;
    }
}