using Microsoft.EntityFrameworkCore;
using OweMe.Application;
using OweMe.Application.Groups;
using OweMe.Application.Personas;
using OweMe.Domain.Expenses;
using OweMe.Domain.Groups;
using OweMe.Domain.Users;
using OweMe.Persistence.Common;

namespace OweMe.Persistence.Data;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    TimeProvider timeProvider,
    IUserContext userContext)
    : AuditableDbContext(options, timeProvider, userContext), IGroupContext, IPersonaContext
{
    public DbSet<Expense> Expenses { get; set; }

    public DbSet<ExpenseSplit> Splits { get; set; }

    public DbSet<Group> Groups { get; set; }

    public DbSet<Persona> Personas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}