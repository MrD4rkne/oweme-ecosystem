using OweMe.Application.Groups.Queries.Get;
using OweMe.Domain.Groups;
using OweMe.Domain.Users;
using Shouldly;

namespace OweMe.Application.UnitTests.Groups;

public class GetGroupResultTests
{
    public static TheoryData<UserId?, DateTimeOffset?> ModifiedByAndAtData => new()
    {
        { UserId.New(), DateTimeOffset.UtcNow },
        { null, null }
    };

    [Theory]
    [MemberData(nameof(ModifiedByAndAtData))]
    public void FromDomain_MapsAllPropertiesCorrectly(UserId? modifiedBy, DateTimeOffset? modifiedAt)
    {
        // Arrange
        var group = new Group
        {
            Id = GroupId.New(),
            Name = "Test Group",
            Description = "Test Description",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = modifiedAt,
            CreatedBy = UserId.New(),
            UpdatedBy = modifiedBy
        };

        // Act
        var dto = GetGroupResult.FromDomain(group);

        // Assert
        dto.Id.ShouldBe(group.Id.Value);
        dto.Name.ShouldBe(group.Name);
        dto.Description.ShouldBe(group.Description);
        dto.CreatedAt.ShouldBe(group.CreatedAt);
        dto.UpdatedAt.ShouldBe(group.UpdatedAt);
        dto.CreatedBy.ShouldBe(group.CreatedBy.Value);
        dto.UpdatedBy.ShouldBe(group.UpdatedBy?.Value);
    }
}