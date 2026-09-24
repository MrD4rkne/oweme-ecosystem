using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using OweMe.Api;
using OweMe.Api.Description;
using OweMe.IntegrationTests.Authentication;
using Shouldly;

namespace OweMe.IntegrationTests;

public sealed class ApiShouldNotRequireAuthentication(OweMeApi api, ITestOutputHelper output) : IClassFixture<OweMeApi>
{
    private readonly WebApplicationFactory<Program> _api = api.WithTestLogging(output);

    [Fact]
    public async Task For_GetHealthEndpoint()
    {
        // Arrange
        var client = _api.CreateClient()
            .AsAnonymous();

        // Act
        var response = await client.GetAsync("/healthz", TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task For_GetLivenessEndpoint()
    {
        // Arrange
        var client = _api.CreateClient()
            .AsAnonymous();

        // Act
        var response = await client.GetAsync("/alive", TestContext.Current.CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task For_GetApiInformationEndpoint()
    {
        // Arrange
        var client = _api.CreateClient()
            .AsAnonymous();

        // Act
        var response =
            await client.GetFromJsonAsync<ApiInformation>("/api/info", TestContext.Current.CancellationToken);

        // Assert
        response.ShouldNotBeNull();
        response.Title.ShouldBe("OweMe API");
        response.Version.ShouldNotBeNullOrWhiteSpace();
        response.Description.ShouldNotBeNullOrWhiteSpace();
        response.BuildVersion.ShouldNotBeNullOrWhiteSpace();
    }
}