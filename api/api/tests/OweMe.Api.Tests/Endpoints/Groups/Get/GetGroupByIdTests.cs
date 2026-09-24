using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using OweMe.Api.Endpoints.Groups.Get;
using OweMe.Application.Common.Exceptions;
using OweMe.Application.Groups.Queries.Get;
using Shouldly;
using Wolverine;

namespace OweMe.Api.Tests.Endpoints.Groups.Get;

public class GetGroupByIdTests
{
    [Fact]
    public async Task GetGroup_ReturnsOk_WhenSuccess()
    {
        // Arrange
        var messageBusMock = new Mock<IMessageBus>();
        var expectedValue = new GetGroupResult(Guid.NewGuid(), "Test Group", "This is a test group.",
            DateTimeOffset.UtcNow, Guid.NewGuid(), DateTimeOffset.UtcNow, Guid.NewGuid());

        messageBusMock.Setup(m =>
                m.InvokeAsync<GetGroupResult>(It.IsAny<GetGroupQuery>(), It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(expectedValue);

        // Act
        var result = await GetGroupByIdEndpoint.GetGroup(expectedValue.Id, messageBusMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<GetGroupResult>>();

        var okResult = result as Ok<GetGroupResult>;
        okResult.ShouldNotBeNull();
        okResult.Value.ShouldNotBeNull();

        okResult.Value.Id.ShouldBe(expectedValue.Id);
        okResult.Value.Name.ShouldBe(expectedValue.Name);
        okResult.Value.Description.ShouldBe(expectedValue.Description);
        okResult.Value.CreatedAt.ShouldBe(expectedValue.CreatedAt);
        okResult.Value.CreatedBy.ShouldBe(expectedValue.CreatedBy);
        okResult.Value.UpdatedAt.ShouldBe(expectedValue.UpdatedAt);
        okResult.Value.UpdatedBy.ShouldBe(expectedValue.UpdatedBy);
    }

    [Fact]
    public async Task GetGroup_ReturnsNotFound_WhenGroupNotFound()
    {
        // Arrange
        var messageBusMock = new Mock<IMessageBus>();
        messageBusMock.Setup(m =>
                m.InvokeAsync<GetGroupResult>(It.IsAny<GetGroupQuery>(), It.IsAny<CancellationToken>(), null))
            .Throws(new NotFoundException("Group not found."));

        var groupId = Guid.NewGuid();

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(async () =>
        {
            await GetGroupByIdEndpoint.GetGroup(groupId, messageBusMock.Object);
        });
    }
}