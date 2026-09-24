using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OweMe.IntegrationTests.Authentication;

public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    internal const string SchemeName = "TestScheme";

    internal const string UserIdHeader = "X-Test-User-Id";
    internal const string UserEmailHeader = "X-Test-UserEmail";
    internal const string ScopeHeader = "X-Test-Scope";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization")) return Task.FromResult(AuthenticateResult.NoResult());

        var userId = Request.Headers[UserIdHeader].FirstOrDefault();
        var userName = Request.Headers[UserEmailHeader].FirstOrDefault();
        var scope = Request.Headers[ScopeHeader].FirstOrDefault();
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(scope))
            return Task.FromResult(AuthenticateResult.Fail("Missing required test authentication headers."));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, userName),
            new Claim("scope", scope)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}