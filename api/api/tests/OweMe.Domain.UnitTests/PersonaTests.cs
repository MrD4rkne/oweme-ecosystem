using OweMe.Domain.Users;
using Shouldly;

namespace OweMe.Domain.UnitTests;

public sealed class PersonaTests
{
    [Fact]
    public void CanUserAccess_ShouldReturnTrue_WhenUserIdMatches()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var persona = new Persona
        {
            Id = new PersonaId(Guid.NewGuid()),
            UserId = userId,
            Name = "Test Persona",
            CreatedBy = new UserId(Guid.NewGuid())
        };

        // Act
        var result = persona.CanUserAccess(userId);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void CanUserAccess_ShouldReturnTrue_WhenCreatedByMatchesButLinkedToAnotherUser()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var persona = new Persona
        {
            Id = new PersonaId(Guid.NewGuid()),
            UserId = new UserId(Guid.NewGuid()),
            Name = "Test Persona",
            CreatedBy = userId
        };

        // Act
        var result = persona.CanUserAccess(userId);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void CanUserAccess_ShouldReturnFalse_WhenUserIdDoesNotMatch()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var anotherUserId = new UserId(Guid.NewGuid());
        var persona = new Persona
        {
            Id = new PersonaId(Guid.NewGuid()),
            UserId = userId,
            Name = "Test Persona",
            CreatedBy = userId
        };

        // Act
        var result = persona.CanUserAccess(anotherUserId);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void CanUserAccess_ShouldReturnTrue_WhenUserIdIsNotSetButCreatedByMatches()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var persona = new Persona
        {
            Id = new PersonaId(Guid.NewGuid()),
            UserId = null,
            Name = "Test Persona",
            CreatedBy = userId
        };

        // Act
        var result = persona.CanUserAccess(userId);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void CanUserAccess_ShouldReturnFalse_WhenUserIdIsNotSetAndCreatedByDoesNotMatch()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var anotherUserId = new UserId(Guid.NewGuid());
        var persona = new Persona
        {
            Id = new PersonaId(Guid.NewGuid()),
            UserId = null,
            Name = "Test Persona",
            CreatedBy = userId
        };

        // Act
        var result = persona.CanUserAccess(anotherUserId);

        // Assert
        result.ShouldBeFalse();
    }
}