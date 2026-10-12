using Moq;
using OweMe.Application.Common.Exceptions;
using OweMe.Application.Groups.Queries.Get;
using OweMe.Domain.Groups;
using OweMe.Domain.Users;
using Shouldly;

namespace OweMe.Application.UnitTests.Groups.Queries.Get;

public class GetGroupQueryHandlerTests : BaseCommandTest
{
    [Fact]
    public async Task Handle_ShouldReturnGroup_WhenGroupExistsAndUserHasAccess()
    {
        // Arrange
        var userId = UserId.New();
        UserContextMock.Setup(x => x.Id).Returns(userId);

        var group = new Group { Id = GroupId.New(), Name = "Test Group", CreatedBy = userId };
        await GroupContextMock.Object.Groups.AddAsync(group, TestContext.Current.CancellationToken);
        await GroupContextMock.Object.SaveChangesAsync(TestContext.Current.CancellationToken);
        var groupId = group.Id;

        GroupContextMock.Invocations.Clear();
        UserContextMock.Invocations.Clear();

        var query = new GetGroupQuery(groupId.Value);

        // Act
        var result = await GetGroupQueryHandler.HandleAsync(query,
            GroupContextMock.Object, UserContextMock.Object,
            TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Test Group");
        result.Id.ShouldBe(groupId.Value);
        result.CreatedBy.ShouldBe<Guid>(userId.Value);
        result.CreatedAt.ShouldBe(group.CreatedAt);
        result.UpdatedBy.ShouldBeNull();
        result.UpdatedAt.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_ShouldThrow_NotFound_WhenGroupDoesNotExist()
    {
        // Arrange
        var groupId = Guid.NewGuid();

        var query = new GetGroupQuery(groupId);

        // Act
        await Assert.ThrowsAsync<NotFoundException>(() => GetGroupQueryHandler.HandleAsync(query,
            GroupContextMock.Object, UserContextMock.Object,
            TestContext.Current.CancellationToken));

        UserContextMock.Verify(x => x.Id, Times.AtMostOnce);
    }

    [Fact]
    public async Task HandleShouldThrow_NotFound_WhenUserDoesNotHaveAccessToGroup()
    {
        // Arrange
        var otherUserId = UserId.New();
        UserContextMock.Setup(x => x.Id).Returns(otherUserId);

        // Let's create a group with a different user
        var group = new Group
            { Id = GroupId.New(), Name = "Test Group", CreatedAt = DateTimeOffset.UtcNow, CreatedBy = otherUserId };
        await GroupContextMock.Object.Groups.AddAsync(group, TestContext.Current.CancellationToken);
        await GroupContextMock.Object.SaveChangesAsync(TestContext.Current.CancellationToken);
        var groupId = group.Id;

        var userId = UserId.New();
        userId.ShouldNotBe(otherUserId);
        UserContextMock.Setup(x => x.Id).Returns(userId);

        UserContextMock.Invocations.Clear();
        GroupContextMock.Invocations.Clear();

        var query = new GetGroupQuery(groupId.Value);

        // Act
        await Assert.ThrowsAsync<NotFoundException>(() => GetGroupQueryHandler.HandleAsync(query,
            GroupContextMock.Object, UserContextMock.Object,
            TestContext.Current.CancellationToken));

        UserContextMock.Verify(x => x.Id, Times.Once);
    }
}