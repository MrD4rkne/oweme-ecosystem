namespace OweMe.Api.SmokeTests.Configuration;

internal sealed record ApiSettings
{
    public const string SectionName = "ApiSettings";

    public required string BaseUrl { get; init; }
}