using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using OweMe.Api;
using OweMe.Application.Groups.Commands.Create;
using OweMe.IntegrationTests.Authentication;
using Shouldly;

namespace OweMe.IntegrationTests;

public sealed class ApiShouldRequireAuthentication(OweMeApi api, ITestOutputHelper output) : IClassFixture<OweMeApi>
{
    private readonly WebApplicationFactory<Program> _api = api.WithTestLogging(output);

    private async Task VerifyEndpointRequiresAuthentication(Func<HttpClient, Task<HttpResponseMessage>> requestFunc)
    {
        // Arrange
        var client = _api.CreateClient()
            .AsAnonymous();

        // Act
        var response = await requestFunc(client);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task For_GetGroupsByIdEndpoint()
    {
        await VerifyEndpointRequiresAuthentication(client => client.GetAsync($"/api/groups/{Guid.NewGuid()}"));
    }

    [Fact]
    public async Task For_CreateGroupEndpoint()
    {
        var group = new CreateGroupCommand
        {
            Name = "Test Group",
            Description = "Test Description"
        };

        await VerifyEndpointRequiresAuthentication(client => client.PostAsync("/api/groups",
            new StringContent(JsonSerializer.Serialize(group), Encoding.UTF8, "application/json")));
    }
}