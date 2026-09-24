using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using OweMe.Api.Endpoints.Ledgers.Create;
using OweMe.Application.Groups.Commands.Create;
using Shouldly;
using Wolverine;

namespace OweMe.Api.Tests.Endpoints.Groups.Create;

public class CreateGroupEndpointTests
{
    [Fact]
    public async Task CreateGroup_ReturnsCreatedResult()
    {
        // Arrange
        var messageBusMock = new Mock<IMessageBus>();
        var request = new CreateGroupCommand
        {
            Name = "Test Group",
            Description = "This is a test group."
        };

        var groupId = Guid.NewGuid();
        var groupCreated = new CreateGroupCommandHandler.GroupCreated(groupId);

        messageBusMock.Setup(m =>
                m.InvokeAsync<CreateGroupCommandHandler.GroupCreated>(It.IsAny<CreateGroupCommand>(),
                    It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(groupCreated);

        // Act
        var result =
            await CreateGroupEndpoint.CreateGroup(request, messageBusMock.Object,
                TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeOfType<Created>();

        var createdResult = result as Created;
        createdResult.ShouldNotBeNull();
        createdResult.Location.ShouldBe($"/api/groups/{groupId}");

        messageBusMock.Verify(m =>
            m.InvokeAsync<CreateGroupCommandHandler.GroupCreated>(
                It.Is<CreateGroupCommand>(command =>
                    command.Name == request.Name && command.Description == request.Description),
                It.IsAny<CancellationToken>(), null), Times.Once);
    }
}