using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using OweMe.Application.Groups.Commands.Create;
using OweMe.Application.Groups.Queries.Get;
using OweMe.IntegrationTests.Authentication;
using Shouldly;

namespace OweMe.IntegrationTests.Groups;

public sealed class CreateGroupEndpointTests(OweMeApi api, ITestOutputHelper output)
{
    private readonly WebApplicationFactory<Program> _api = api.WithTestLogging(output);

    [Fact]
    public async Task For_ValidRequest_Should_ReturnCreated_WithLocationOfTheGroup()
    {
        // Arrange
        var client = _api.CreateClient().AsUser(new TestUser());
        var command = new CreateGroupCommand { Name = "Test Group", Description = "Test Description" };

        // Act
        var response = await client.PostAsJsonAsync("/api/groups", command, TestContext.Current.CancellationToken);
        var groupId =
            await response.Content.ReadFromJsonAsync<CreateGroupCommandHandler.GroupCreated>(
                cancellationToken: TestContext.Current.CancellationToken);
        groupId.ShouldNotBeNull();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.ShouldBe(new Uri($"/api/groups/{groupId.Id}", UriKind.Relative));

        var group = await client.GetFromJsonAsync<GetGroupResult>(response.Headers.Location,
            TestContext.Current.CancellationToken);
        group.ShouldNotBeNull();
        group.Id.ShouldBe(groupId.Id);
        group.Name.ShouldBe(command.Name);
        group.Description.ShouldBe(command.Description);
    }

    [Fact]
    public async Task For_InvalidRequest_Should_ReturnBadRequest()
    {
        // Arrange
        var client = _api.CreateClient().AsUser(new TestUser());
        var command = new CreateGroupCommand { Name = "" };

        // Act
        var response = await client.PostAsJsonAsync("/api/groups", command, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}