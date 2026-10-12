using OweMe.Application.Groups.Commands.Create;
using OweMe.Domain.Users;
using OweMe.Tests.Common;
using Shouldly;

namespace OweMe.Application.UnitTests.Groups.Commands.Create;

public class CreateGroupCommandHandlerTests : BaseCommandTest
{
    private readonly DateTimeOffset _currentTime = DateTimeOffset.UtcNow;
    private readonly UserId _currentUserId = UserId.New();

    public CreateGroupCommandHandlerTests()
    {
        UserContextMock.Setup(x => x.Id).Returns(_currentUserId);
        TimeProvider.Setup(x => x.GetUtcNow()).Returns(_currentTime);
    }

    [Fact]
    public async Task Handle_ShouldCreateGroup_WhenValidCommand()
    {
        // Arrange
        const string groupName = "Test Group";
        const string groupDescription = "This is a test group.";
        var command = new CreateGroupCommand
        {
            Name = groupName,
            Description = groupDescription
        };

        // Act
        var result = await CreateGroupCommandHandler.Handle(command, GroupContextMock.Object, TimeProvider.Object,
            TestContext.Current.CancellationToken);

        // Assert
        var addedGroup = GroupContextMock.Object.Groups
            .FirstOrDefault(x => x.Name == groupName && x.Description == groupDescription);
        addedGroup.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.Id.ShouldBe(addedGroup.Id.Value);

        addedGroup.Name.ShouldBe(command.Name);
        addedGroup.Description.ShouldBe(command.Description);

        addedGroup.ShouldBeCreated(
            _currentUserId,
            _currentTime
        );
        addedGroup.ShouldBeNeverUpdated();
    }
}