using Microsoft.Extensions.DependencyInjection;
using Moq;
using OweMe.Application.User;
using OweMe.Domain.Users;

namespace OweMe.Application.UnitTests.User;

public class ServiceProviderExtensionsTests
{
    private readonly ServiceCollection _services;
    private readonly Mock<IUserContextSetter> _userContextSetterMock = new();

    public ServiceProviderExtensionsTests()
    {
        _services = new ServiceCollection();
        _services.AddSingleton(_userContextSetterMock.Object);
    }

    [Fact]
    public void SetUserContext_ShouldCallSetContextOnUserContextSetter()
    {
        // Arrange
        var serviceProvider = _services.BuildServiceProvider();
        var userId = new UserId(Guid.NewGuid());
        var email = "test@example.com";

        // Act
        serviceProvider.SetUserContext(userId, email);

        // Assert
        _userContextSetterMock.Verify(x => x.SetContext(userId, email), Times.Once);
    }

    [Fact]
    public void SetUserContext_ShouldThrowArgumentNullException_WhenServiceProviderIsNull()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var email = "test@example.com";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((IServiceProvider)null!).SetUserContext(userId, email));
    }

    [Fact]
    public void SetUserContext_ShouldThrowArgumentException_WhenUserIdIsDefault()
    {
        // Arrange
        var serviceProvider = _services.BuildServiceProvider();
        var userId = default(UserId);
        var email = "test@example.com";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => serviceProvider.SetUserContext(userId, email));
    }

    [Fact]
    public void SetUserContext_ShouldThrowArgumentException_WhenEmailIsNullOrEmpty()
    {
        // Arrange
        var serviceProvider = _services.BuildServiceProvider();
        var userId = new UserId(Guid.NewGuid());
        var email = string.Empty;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => serviceProvider.SetUserContext(userId, email));
    }
}