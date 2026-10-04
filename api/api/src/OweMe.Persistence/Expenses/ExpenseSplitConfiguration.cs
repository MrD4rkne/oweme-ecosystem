using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OweMe.Domain.Expenses;
using OweMe.Domain.Users;

namespace OweMe.Persistence.Expenses;

internal sealed class ExpenseSplitConfiguration : IEntityTypeConfiguration<ExpenseSplit>
{
    public void Configure(EntityTypeBuilder<ExpenseSplit> modelBuilder)
    {
        modelBuilder.HasKey(expenseSplit => new { expenseSplit.ExpenseId, expenseSplit.PersonaId });

        modelBuilder.Property(expenseSplit => expenseSplit.Amount)
            .IsRequired();

        modelBuilder.Property(expenseSplit => expenseSplit.ExpenseId)
            .IsRequired();

        modelBuilder.Property(expenseSplit => expenseSplit.PersonaId)
            .IsRequired();

        modelBuilder.HasOne<Expense>()
            .WithMany()
            .HasForeignKey(expenseSplit => expenseSplit.ExpenseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.HasOne<Persona>()
            .WithMany()
            .HasForeignKey(expenseSplit => expenseSplit.PersonaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}