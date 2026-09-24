using OweMe.Api.Client;
using OweMe.Api.SmokeTests.Configuration;
using OweMe.Api.SmokeTests.Helpers;
using Shouldly;

namespace OweMe.Api.SmokeTests.Endpoints.Groups;

public class CreateGroupEndpointTests(OweMeClientFixture fixture)
{
    private readonly CreateGroupCommand _validCreateGroupRequest = new()
    {
        Name = "Test Group",
        Description = "This is a test group."
    };

    [Fact]
    public async Task For_ValidRequest_Should_CreateGroupSuccessfully()
    {
        // Arrange
        var client = fixture.GetClient(OweMeClientFixture.AuthenticatedClientKey);

        // Act
        var groupId =
            await CreateGroupHelper.CreateGroup(client, _validCreateGroupRequest,
                TestContext.Current.CancellationToken);

        // Assert
        groupId.ShouldNotBe(Guid.Empty, "Group ID should not be empty after successful creation.");

        var group = await client.GetGroupAsync(groupId, TestContext.Current.CancellationToken);
        group.ShouldNotBeNull("Group should not be null after creation.");
        group.Result.ShouldNotBeNull("Group result should not be null.");
        group.Result.Id.ShouldBe(groupId, "Group ID should match the created group ID.");
        group.Result.Name.ShouldBe(_validCreateGroupRequest.Name, "Group name should match the request.");
        group.Result.Description.ShouldBe(_validCreateGroupRequest.Description,
            "Group description should match the request.");
        group.Result.CreatedBy.ShouldNotBe(Guid.Empty, "Group created by should not be empty.");
        group.Result.UpdatedAt.ShouldBeNull("Group updated date should be null for a newly created group.");
        group.Result.UpdatedBy.ShouldBeNull("Group updated by should be null for a newly created group.");
    }

    [Fact]
    public async Task For_InvalidRequest_Should_ReturnBadRequest()
    {
        // Arrange
        var client = fixture.GetClient(OweMeClientFixture.AuthenticatedClientKey);

        var invalidCreateGroupRequest = new CreateGroupCommand
        {
            Name = "", // Invalid: Name cannot be empty
            Description = "This is an invalid test group."
        };

        // Act
        var apiException = await Should.ThrowAsync<ApiException>(() =>
            client.CreateGroupAsync(invalidCreateGroupRequest, TestContext.Current.CancellationToken));

        // Assert
        apiException.ShouldNotBeNull("API exception should not be null for invalid request.");
        apiException.StatusCode.ShouldBe(400, "Expected status code 400 Bad Request for invalid request.");
    }

    [Fact]
    public async Task For_UnauthorizedUser_ShouldReturn_Unauthorized()
    {
        // Arrange
        var client = fixture.GetClient(OweMeClientFixture.UnauthenticatedClientKey);

        // Act
        var apiException = await Should.ThrowAsync<ApiException>(() =>
            client.CreateGroupAsync(_validCreateGroupRequest, TestContext.Current.CancellationToken));

        // Assert
        apiException.ShouldNotBeNull("API exception should not be null for unauthorized request.");
        apiException.StatusCode.ShouldBe(401, "Expected status code 401 Unauthorized for unauthenticated request.");
    }
}