using System.Reflection;
using NetArchTest.Rules;

namespace OweMe.Architecture.Tests;

public class ProjectsReferencesTests
{
    private const string ApplicationNamespace = "OweMe.Application";
    private const string DomainNamespace = "OweMe.Domain";
    private const string InfrastructureNamespace = "OweMe.Infrastructure";
    private const string PersistenceNamespace = "OweMe.Persistence";
    private const string PresentationNamespace = "OweMe.Api";

    [Fact]
    public void Domain_ShouldNot_HaveDependencyOnApplicationProject()
    {
        // Arrange
        var domainAssembly = Assembly.Load(DomainNamespace);

        // Act
        var result = Types.InAssembly(domainAssembly)
            .That()
            .ResideInNamespace(DomainNamespace)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, "Domain project has dependencies on Application project.");
    }

    [Theory]
    [InlineData(DomainNamespace, new[] { ApplicationNamespace, InfrastructureNamespace, PersistenceNamespace, PresentationNamespace })]
    [InlineData(ApplicationNamespace, new[] { InfrastructureNamespace, PersistenceNamespace, PresentationNamespace })]
    [InlineData(InfrastructureNamespace, new[] { PersistenceNamespace, PresentationNamespace })]
    [InlineData(PersistenceNamespace, new[] { InfrastructureNamespace, PresentationNamespace })]
    public void Project_ShouldNot_HaveDependencyOnOuterProjects(string projectName, string[] forbidden)
    {
        // Arrange
        var assembly = Assembly.Load(projectName);

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace(projectName)
            .ShouldNot()
            .HaveDependencyOnAny(forbidden)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"{projectName} depends on outer projects: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
