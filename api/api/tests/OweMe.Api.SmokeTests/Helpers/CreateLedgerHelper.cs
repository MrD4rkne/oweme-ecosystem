using OweMe.Api.Client;
using Shouldly;

namespace OweMe.Api.SmokeTests.Helpers;

public static class CreateGroupHelper
{
    public static async Task<Guid> CreateGroup(OweMeApiClient client,
        CreateGroupCommand createGroupRequest,
        CancellationToken cancellationToken = default)
    {
        var response = await client.CreateGroupAsync(createGroupRequest, cancellationToken);

        // Assert
        response.ShouldNotBeNull();
        response.StatusCode.ShouldBe(201, "Expected status code 201 Created for successful group creation.");
        response.Headers.ShouldNotBeNull();

        return response.Headers.GetLocationHeaderValue("/api/groups/", Guid.Parse);
    }
}