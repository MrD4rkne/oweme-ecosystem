namespace OweMe.Api.SmokeTests.Http;

public interface ITokenManager
{
    public Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}