using OweMe.Application.Groups.Commands.AddMember;
using OweMe.Domain.Groups;
using OweMe.Domain.Users;
using Shouldly;

namespace OweMe.Application.UnitTests.Groups.Commands.AddMember;

public class AddGroupMemberCommandValidatorTests
{
    private readonly AddGroupMemberCommandValidator _validator = new();

    [Fact]
    public void Should_Validate_AddGroupMemberCommand()
    {
        // Arrange
        var command = new AddGroupMemberCommand(new GroupId(Guid.NewGuid()), new PersonaId(Guid.NewGuid()));

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeTrue("Validation should pass for valid command.");
    }

    [Fact]
    public void Should_Invalidate_AddGroupMemberCommand_When_PersonaId_Is_Empty()
    {
        // Arrange
        var command = new AddGroupMemberCommand(new GroupId(Guid.NewGuid()), PersonaId.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse("Validation should fail when persona id is empty.");
        result.Errors.Any(e => e.PropertyName == nameof(command.PersonaId)).ShouldBeTrue();
    }
}
