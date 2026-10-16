using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using OweMe.Application.Groups.Commands.Create;
using OweMe.IntegrationTests.Authentication;
using Shouldly;

namespace OweMe.IntegrationTests.Groups;

public sealed class GetGroupByIdEndpointTests(OweMeApi api, ITestOutputHelper output)
{
    private readonly WebApplicationFactory<Program> _api = api.WithTestLogging(output);

    [Fact]
    public async Task For_NonExistingGroup_Should_ReturnNotFound()
    {
        // Arrange
        var client = _api.CreateClient().AsUser(new TestUser());

        // Act
        var response = await client.GetAsync($"/api/groups/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task For_GroupOfAnotherUser_Should_ReturnNotFound()
    {
        // Arrange
        var owner = _api.CreateClient().AsUser(new TestUser());
        var created = await owner.PostAsJsonAsync("/api/groups", new CreateGroupCommand { Name = "Test Group" },
            TestContext.Current.CancellationToken);
        created.EnsureSuccessStatusCode();

        var otherUser = _api.CreateClient().AsUser(new TestUser());

        // Act
        var response = await otherUser.GetAsync(created.Headers.Location, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}