using Microsoft.EntityFrameworkCore;
using Moq;
using OweMe.Application;
using OweMe.Domain.Groups;
using OweMe.Persistence.Groups;
using OweMe.Tests.Common;
using Shouldly;

namespace OweMe.Persistence.Tests.Groups;

public class GroupMigrationsTests() : PostgresTestBase("oweme_migrations_test"), IAsyncLifetime
{
    private readonly Mock<TimeProvider> _timeProvider = new();
    private readonly Mock<IUserContext> _userContextMock = new();

    public async ValueTask InitializeAsync()
    {
        await SetupAsync();
    }

    [Fact]
    public async Task Migrations_ShouldApplySuccessfully()
    {
        // Arrange
        await using var context = new GroupDbContext(
            new DbContextOptionsBuilder<GroupDbContext>()
                .UseNpgsql(ConnectionString)
                .Options,
            _timeProvider.Object,
            _userContextMock.Object
        );

        // Act
        await context.Database.MigrateAsync(TestContext.Current.CancellationToken);

        // Assert
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync(TestContext.Current.CancellationToken);
        pendingMigrations.ShouldBeEmpty("There should be no pending migrations after applying them.");
    }

    [Fact]
    public async Task Migrations_ShouldBeAbleToCreateAndQuery()
    {
        // Arrange
        await using var context = new GroupDbContext(
            new DbContextOptionsBuilder<GroupDbContext>()
                .UseNpgsql(ConnectionString)
                .Options,
            _timeProvider.Object,
            _userContextMock.Object
        );

        await context.Database.MigrateAsync(TestContext.Current.CancellationToken);

        var group = new Group
        {
            Name = "Test Group",
            Description = "This is a test group."
        };

        // Act
        context.Ledgers.Add(group);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var createdGroup = await context.Ledgers
            .FirstOrDefaultAsync(x => x.Name == "Test Group" && x.Description == "This is a test group.",
                TestContext.Current.CancellationToken);

        // Assert
        createdGroup.ShouldNotBeNull("The group should have been created successfully.");
        createdGroup.Name.ShouldBe("Test Group");
        createdGroup.Description.ShouldBe("This is a test group.");
        createdGroup.ShouldBeCreated(
            _userContextMock.Object.Id,
            _timeProvider.Object.GetUtcNow()
        );
    }
}