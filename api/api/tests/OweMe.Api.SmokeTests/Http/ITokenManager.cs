namespace OweMe.Api.SmokeTests.Http;

public interface ITokenManager
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}