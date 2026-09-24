using OweMe.Application.Groups.Commands.Create;
using OweMe.Tests.Common;
using Shouldly;

namespace OweMe.Application.UnitTests.Groups.Commands.Create;

public class CreateGroupCommandHandlerTests : BaseCommandTest
{
    private readonly DateTimeOffset _currentTime = DateTimeOffset.UtcNow;
    private readonly Guid _currentUserId = Guid.NewGuid();

    public CreateGroupCommandHandlerTests()
    {
        _userContextMock.Setup(x => x.Id).Returns(_currentUserId);
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(_currentTime);
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
        var result = await CreateGroupCommandHandler.Handle(command, _groupContextMock.Object,
            TestContext.Current.CancellationToken);

        // Assert
        var addedGroup = _groupContextMock.Object.Groups
            .FirstOrDefault(x => x.Name == groupName && x.Description == groupDescription);
        addedGroup.ShouldNotBeNull();
        result.ShouldNotBeNull();
        result.Id.ShouldBe(addedGroup.Id);

        addedGroup.Name.ShouldBe(command.Name);
        addedGroup.Description.ShouldBe(command.Description);

        addedGroup.ShouldBeCreated(
            _currentUserId,
            _currentTime
        );
        addedGroup.ShouldBeNeverUpdated();
    }
}