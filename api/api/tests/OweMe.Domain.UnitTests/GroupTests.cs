using OweMe.Domain.Groups;
using OweMe.Domain.Users;
using OweMe.Tests.Common;
using Shouldly;

namespace OweMe.Domain.UnitTests;

public class GroupTests
{
    [Fact]
    public void CanUserAccess_Creator_ShouldReturnTrue()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var group = new Group
        {
            CreatedBy = userId,
            Name = "Test Group",
            Id = Guid.NewGuid(),
        };

        // Act
        var canAccess = group.CanUserAccess(userId);

        // Assert
        canAccess.ShouldBeTrue();
    }

    [Fact]
    public void CanUserAccess_OtherUser_ShouldReturnFalse()
    {
        // Arrange
        var creatorId = new UserId(Guid.NewGuid());
        var otherUserId = new UserId(GuidHelper.CreateDifferentGuid(creatorId));

        var group = new Group
        {
            CreatedBy = creatorId,
            Name = "Test Group",
            Id = Guid.NewGuid()
        };

        // Act
        var canAccess = group.CanUserAccess(otherUserId);

        // Assert
        canAccess.ShouldBeFalse();
    }
}