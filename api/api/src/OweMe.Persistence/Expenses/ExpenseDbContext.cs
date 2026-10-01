using Microsoft.EntityFrameworkCore;
using OweMe.Application;
using OweMe.Application.Expenses;
using OweMe.Domain.Expenses;
using OweMe.Domain.Groups;
using OweMe.Domain.Users;
using OweMe.Persistence.Common;

namespace OweMe.Persistence.Expenses;

public sealed class ExpenseDbContext(
    DbContextOptions<ExpenseDbContext> options,
    TimeProvider timeProvider,
    IUserContext userContext)
    : AuditableDbContext(options, timeProvider, userContext), IExpensesContext
{
    public DbSet<Expense> Expenses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Expense>()
            .HasKey(l => l.Id);

        modelBuilder.Entity<Expense>()
            .Property(g => g.Id);

        modelBuilder.Entity<Expense>()
            .Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(ExpenseConstants.MaxNameLength);
        
        modelBuilder.Entity<ExpenseSplit>()
            .HasKey(es => new { es.ExpenseId, es.PersonaId });
        
        modelBuilder.Entity<ExpenseSplit>()
            .Property(es => es.Amount)
            .IsRequired();

        modelBuilder.Entity<ExpenseSplit>()
            .Property(es => es.ExpenseId)
            .IsRequired();

        modelBuilder.Entity<ExpenseSplit>()
            .Property(es => es.PersonaId)
            .IsRequired();
        
        modelBuilder.Entity<ExpenseSplit>()
            .HasOne<Expense>()
            .WithMany()
            .HasForeignKey(es => es.ExpenseId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<ExpenseSplit>()
            .HasOne<Persona>()
            .WithMany()
            .HasForeignKey(es => es.PersonaId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}