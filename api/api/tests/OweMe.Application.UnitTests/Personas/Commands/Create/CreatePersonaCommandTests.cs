using OweMe.Application.Personas.Commands.Create;
using OweMe.Domain.Users;
using Shouldly;

namespace OweMe.Application.UnitTests.Personas.Commands.Create;

public sealed class CreatePersonaCommandTests : BaseCommandTest
{
    private readonly DateTimeOffset _currentTime = DateTimeOffset.UtcNow;
    private readonly UserId _currentUserId = UserId.New();

    public CreatePersonaCommandTests()
    {
        UserContextMock.Setup(x => x.Id).Returns(_currentUserId);
        TimeProvider.Setup(x => x.GetUtcNow()).Returns(_currentTime);
    }

    [Fact]
    public async Task Should_CreatePersona()
    {
        // Arrange
        const string personaName = "SomeNewPersonaName";
        
        var createPersonaCommand = new CreatePersonaCommand()
        {
            Name = personaName,
        };
        
        // Act
        var result = await CreatePersonaCommandHandler.Handle(
            createPersonaCommand, PersonaContext.Object, TimeProvider.Object,
            cancellationToken: TestContext.Current.CancellationToken
        );
        
        // Assert
        result.ShouldNotBeNull();
        result.Id.Value.ShouldNotBe(Guid.Empty);
    } 
}