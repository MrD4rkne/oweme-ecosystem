namespace OweMe.Api.SmokeTests.Configuration;

internal sealed record UserSettings
{
    public const string SectionName = "UserSettings";

    public required string Username { get; init; }

    public required string Password { get; init; }

    public required string Scope { get; init; }
}