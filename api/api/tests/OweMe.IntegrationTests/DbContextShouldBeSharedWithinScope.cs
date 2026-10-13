using Microsoft.Extensions.DependencyInjection;
using OweMe.Application.Groups;
using OweMe.Application.Personas;
using OweMe.Persistence.Data;
using Shouldly;

namespace OweMe.IntegrationTests;

public sealed class DbContextShouldBeSharedWithinScope(OweMeApi api)
{
    [Fact]
    public void For_AllContextInterfaces()
    {
        // Arrange
        using var scope = api.Services.CreateScope();

        // Act
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Assert
        scope.ServiceProvider.GetRequiredService<IGroupContext>().ShouldBeSameAs(dbContext);
        scope.ServiceProvider.GetRequiredService<IPersonaContext>().ShouldBeSameAs(dbContext);
    }
}