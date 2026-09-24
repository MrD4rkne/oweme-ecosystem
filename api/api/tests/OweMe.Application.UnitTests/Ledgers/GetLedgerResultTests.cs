using OweMe.Application.Groups.Queries.Get;
using OweMe.Domain.Groups;
using Shouldly;

namespace OweMe.Application.UnitTests.Groups;

public class GetGroupResultTests
{
    public static TheoryData<Guid?, DateTimeOffset?> ModifiedByAndAtData => new()
    {
        { Guid.NewGuid(), DateTimeOffset.UtcNow },
        { null, null }
    };

    [Theory]
    [MemberData(nameof(ModifiedByAndAtData))]
    public void FromDomain_MapsAllPropertiesCorrectly(Guid? modifiedBy, DateTimeOffset? modifiedAt)
    {
        // Arrange
        var group = new Group
        {
            Id = Guid.NewGuid(),
            Name = "Test Group",
            Description = "Test Description",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = modifiedAt,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = modifiedBy
        };

        // Act
        var dto = GetGroupResult.FromDomain(group);

        // Assert
        dto.Id.ShouldBe(group.Id);
        dto.Name.ShouldBe(group.Name);
        dto.Description.ShouldBe(group.Description);
        dto.CreatedAt.ShouldBe(group.CreatedAt);
        dto.UpdatedAt.ShouldBe(group.UpdatedAt);
        dto.CreatedBy.ShouldBe(group.CreatedBy.Id);
        dto.UpdatedBy.ShouldBe(group.UpdatedBy?.Id);
    }
}