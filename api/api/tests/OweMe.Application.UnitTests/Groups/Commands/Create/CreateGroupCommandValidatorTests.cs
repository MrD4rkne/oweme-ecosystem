using OweMe.Application.Groups.Commands.Create;
using Shouldly;

namespace OweMe.Application.UnitTests.Groups.Commands.Create;

public class CreateGroupCommandValidatorTests
{
    private const int MaxNameLength = 100;
    private const int MaxDescriptionLength = 500;

    private static readonly string[] ValidNames =
    [
        new('A', MaxNameLength),
        "Abcefghijklmnopqrstuvwxyz",
        "test"
    ];

    private static readonly string?[] InvalidNames =
    [
        new('A', MaxNameLength + 1), // Exceeding max length
        string.Empty,
        null
    ];

    private static readonly string?[] ValidDescriptions =
    [
        new('A', MaxDescriptionLength),
        "Abcefghijklmnopqrstuvwxyz",
        string.Empty,
        null
    ];

    private static readonly string[] InvalidDescriptions =
    [
        new('A', MaxDescriptionLength + 1), // Exceeding max length
        new('A', MaxDescriptionLength + 2),
        new('A', MaxDescriptionLength + 10)
    ];

    public static TheoryData<string, string?> ValidNameDescriptionCombinations()
    {
        var data = new TheoryData<string, string?>();
        foreach (var name in ValidNames)
        foreach (var description in ValidDescriptions)
            data.Add(name, description);

        return data;
    }

    [Theory]
    [MemberData(nameof(ValidNameDescriptionCombinations))]
    public void Should_Validate_CreateGroupCommand(string name, string? description)
    {
        // Arrange
        var validator = new CreateGroupCommandValidator();
        var command = new CreateGroupCommand
        {
            Name = name,
            Description = description
        };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeTrue("Validation should pass for valid command.");
    }

    public static TheoryData<string?, string?> InvalidNameValidDescriptionCombinations()
    {
        var data = new TheoryData<string?, string?>();
        foreach (var name in InvalidNames)
        foreach (var description in ValidDescriptions)
            data.Add(name, description);

        return data;
    }

    [Theory]
    [MemberData(nameof(InvalidNameValidDescriptionCombinations))]
    public void Should_Invalidate_CreateGroupCommand_When_Name_Is_Invalid(string? name, string? description)
    {
        // Arrange
        var validator = new CreateGroupCommandValidator();
        var command = new CreateGroupCommand
        {
            Name = name!,
            Description = description
        };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse("Validation should fail when name is empty.");
        result.Errors.Any(e => e.PropertyName == nameof(command.Name)).ShouldBeTrue();
    }

    public static TheoryData<string, string> ValidNameInvalidDescriptionCombinations()
    {
        var data = new TheoryData<string, string>();
        foreach (var name in ValidNames)
        foreach (var description in InvalidDescriptions)
            data.Add(name, description);

        return data;
    }

    [Theory]
    [MemberData(nameof(ValidNameInvalidDescriptionCombinations))]
    public void Should_Invalidate_CreateGroupCommand_When_Description_Exceeds_Max_Length(string name,
        string? description)
    {
        // Arrange
        var validator = new CreateGroupCommandValidator();
        var command = new CreateGroupCommand
        {
            Name = name,
            Description = description
        };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse("Validation should fail when description exceeds maximum length.");
        result.Errors.Any(e => e.PropertyName == nameof(command.Description)).ShouldBeTrue();
    }
}