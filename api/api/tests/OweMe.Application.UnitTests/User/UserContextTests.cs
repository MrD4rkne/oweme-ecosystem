using Microsoft.Extensions.DependencyInjection;
using OweMe.Application.User;
using OweMe.Domain.Users;

namespace OweMe.Application.UnitTests.User;

/// <summary>
/// Validates the behavior of the UserContext class and its related components, ensuring that user context management functions correctly within the application.
/// </summary>
public class UserContextTests
{
    private readonly IServiceProvider _serviceProvider;

    public UserContextTests()
    {
        var services = new ServiceCollection();
        services.AddUserContext();
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void UserContext_ShouldThrowException_WhenAccessedWithoutSettingContext()
    {
        // Arrange
        var userContext = _serviceProvider.GetRequiredService<IUserContext>();

        // Act & Assert
        Assert.Throws<UserContextNotAvailableException>(() => _ = userContext.Id);
        Assert.Throws<UserContextNotAvailableException>(() => _ = userContext.Email);
    }

    [Fact]
    public void UserContext_ShouldWork_WhenContextIsSet()
    {
        // Arrange
        var userContext = _serviceProvider.GetRequiredService<IUserContext>();
        var userId = new UserId(Guid.NewGuid());
        var email = "test@example.com";

        // Act
        _serviceProvider.SetUserContext(userId, email);

        // Assert
        Assert.Equal(userId, userContext.Id);
        Assert.Equal(email, userContext.Email);
    }

    [Fact]
    public void UserContext_CanBeCopiedAcrossScopes_AndRetainsOriginalContext()
    {
        // Arrange
        const string email = "test@example.com";
        var userId = new UserId(Guid.NewGuid());
        _serviceProvider.SetUserContext(userId, email);
        using var scope = _serviceProvider.CreateScope();

        // Act
        scope.ServiceProvider.SetUserContext(new UserId(Guid.NewGuid()), "test2@example.com");

        // Assert
        var userContext = scope.ServiceProvider.GetRequiredService<IUserContext>();
        Assert.NotNull(userContext);
        Assert.Equal(userId, userContext.Id);
        Assert.Equal("test@example.com", userContext.Email);
    }

    [Fact]
    public void UserContext_CanBeReset()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var email = "test@example.com";
        _serviceProvider.SetUserContext(userId, email);

        // Sanity check to ensure context is set.
        var userContextBeforeReset = _serviceProvider.GetRequiredService<IUserContext>();
        Assert.Equal(userId, userContextBeforeReset.Id);
        Assert.Equal(email, userContextBeforeReset.Email);

        // Act
        _serviceProvider.ResetUserContext();

        // Assert
        var userContext = _serviceProvider.GetRequiredService<IUserContext>();
        Assert.Throws<UserContextNotAvailableException>(() => _ = userContext.Id);
        Assert.Throws<UserContextNotAvailableException>(() => { _ = userContext.Email; });
    }
}