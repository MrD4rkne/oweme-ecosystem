using OweMe.Application.Personas.Commands.Create;
using OweMe.Domain.Users;
using OweMe.Tests.Common;
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
            createPersonaCommand, PersonaContext, TimeProvider.Object,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        result.ShouldNotBeNull();
        result.Id.Value.ShouldNotBe(Guid.Empty);
        result.Id.Value.Version.ShouldBe(7);

        var addedPersona = PersonaContext.Personas
            .FirstOrDefault(x => x.Id == result.Id);
        addedPersona.ShouldNotBeNull();
        addedPersona.Name.ShouldBe(personaName);
        addedPersona.ShouldBeCreated(
            _currentUserId,
            _currentTime
        );
        addedPersona.ShouldBeNeverUpdated();
    }
}