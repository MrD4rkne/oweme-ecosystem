using System.Reflection;
using NetArchTest.Rules;

namespace OweMe.Architecture.Tests;

public class ApplicationConventionsTests
{
    private const string ApplicationNamespace = "OweMe.Application";

    [Fact]
    public void EveryCommand_Should_HaveValidator()
    {
        // Arrange
        var assembly = Assembly.Load(ApplicationNamespace);

        var validators = Types.InAssembly(assembly)
            .That()
            .HaveNameEndingWith("CommandValidator")
            .GetTypes()
            .Select(t => t.Name)
            .ToHashSet();

        // Act
        var commandsWithoutValidator = Types.InAssembly(assembly)
            .That()
            .HaveNameEndingWith("Command")
            .GetTypes()
            .Where(t => !validators.Contains($"{t.Name}Validator"))
            .Select(t => t.FullName)
            .ToList();

        // Assert
        Assert.Empty(commandsWithoutValidator);
    }
}
