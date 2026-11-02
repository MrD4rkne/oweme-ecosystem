using EntityFramework.Exceptions.Common;
using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Moq;
using OweMe.Application;
using OweMe.Domain.Expenses;
using OweMe.Domain.Groups;
using OweMe.Domain.Users;
using OweMe.Persistence.Data;
using OweMe.Tests.Common;
using Shouldly;

namespace OweMe.Persistence.Tests.Expenses;

public class ExpenseMigrationsTests() : PostgresTestBase("oweme_expense_migrations_test"), IAsyncLifetime
{
    private readonly Mock<TimeProvider> _timeProvider = new();
    private readonly Mock<IUserContext> _userContextMock = new();

    public async ValueTask InitializeAsync()
    {
        await SetupAsync();

        await using var context = CreateContext();
        await context.Database.MigrateAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DeletingPersona_WithSplits_ShouldFail()
    {
        // Arrange
        var persona = await SeedPersonaAsync(withSplit: true);

        // Act
        // Fresh context: with the splits untracked, EF sends the DELETE and the database constraint decides.
        await using var context = CreateContext();
        context.Personas.Remove(persona);
        var delete = () => context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        await delete.ShouldThrowAsync<ReferenceConstraintException>();
    }

    [Fact]
    public async Task DeletingPersona_WithoutSplits_ShouldSucceed()
    {
        // Arrange
        var persona = await SeedPersonaAsync(withSplit: false);

        // Act
        await using var context = CreateContext();
        context.Personas.Remove(persona);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        (await context.Personas.AnyAsync(p => p.Id == persona.Id, TestContext.Current.CancellationToken))
            .ShouldBeFalse();
    }

    private async Task<Persona> SeedPersonaAsync(bool withSplit)
    {
        await using var context = CreateContext();

        var persona = new Persona { Id = PersonaId.New(), UserId = null, Name = "Alice" };
        context.Personas.Add(persona);

        if (withSplit)
        {
            var group = new Group { Id = GroupId.New(), Name = "Trip" };
            var expense = new Expense { Id = ExpenseId.New(), Name = "Dinner", GroupId = group.Id };
            context.Groups.Add(group);
            context.Expenses.Add(expense);
            context.Splits.Add(new ExpenseSplit { ExpenseId = expense.Id, PersonaId = persona.Id, Amount = 12.34m });
        }

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        return persona;
    }

    private ApplicationDbContext CreateContext() => new(
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ConnectionString)
            .UseExceptionProcessor()
            .Options,
        _timeProvider.Object,
        _userContextMock.Object
    );
}