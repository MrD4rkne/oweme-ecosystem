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
        _userContextMock.Setup(x => x.Id).Returns(userId);

        var group = new Group { Name = "Test Group", CreatedBy = userId };
        await _groupContextMock.Object.Groups.AddAsync(group, TestContext.Current.CancellationToken);
        await _groupContextMock.Object.SaveChangesAsync(TestContext.Current.CancellationToken);
        var groupId = group.Id;

        _groupContextMock.Invocations.Clear();
        _userContextMock.Invocations.Clear();

        var query = new GetGroupQuery(groupId);

        // Act
        var result = await GetGroupQueryHandler.HandleAsync(query,
            _groupContextMock.Object, _userContextMock.Object,
            TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Test Group");
        result.Id.ShouldBe(groupId);
        result.CreatedBy.ShouldBe<Guid>(userId);
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
            _groupContextMock.Object, _userContextMock.Object,
            TestContext.Current.CancellationToken));

        _userContextMock.Verify(x => x.Id, Times.AtMostOnce);
    }

    [Fact]
    public async Task HandleShouldThrow_NotFound_WhenUserDoesNotHaveAccessToGroup()
    {
        // Arrange
        var otherUserId = UserId.New();
        _userContextMock.Setup(x => x.Id).Returns(otherUserId);

        // Let's create a group with a different user
        var group = new Group { Name = "Test Group", CreatedAt = DateTimeOffset.UtcNow, CreatedBy = otherUserId };
        await _groupContextMock.Object.Groups.AddAsync(group, TestContext.Current.CancellationToken);
        await _groupContextMock.Object.SaveChangesAsync(TestContext.Current.CancellationToken);
        var groupId = group.Id;

        var userId = UserId.New();
        userId.ShouldNotBe(otherUserId);
        _userContextMock.Setup(x => x.Id).Returns(userId);

        _userContextMock.Invocations.Clear();
        _groupContextMock.Invocations.Clear();

        var query = new GetGroupQuery(groupId);

        // Act
        await Assert.ThrowsAsync<NotFoundException>(() => GetGroupQueryHandler.HandleAsync(query,
            _groupContextMock.Object, _userContextMock.Object,
            TestContext.Current.CancellationToken));

        _userContextMock.Verify(x => x.Id, Times.Once);
    }
}