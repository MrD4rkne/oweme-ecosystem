using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OweMe.Application.Expenses;
using OweMe.Domain.Expenses;

namespace OweMe.Persistence.Expenses;

internal sealed class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> modelBuilder)
    {
        modelBuilder.HasKey(expense => expense.Id);

        modelBuilder.Property(expense => expense.Id)
            .HasConversion<ExpenseIdConverter>();

        modelBuilder.Property(expense => expense.Name)
            .IsRequired()
            .HasMaxLength(ExpenseConstants.MaxNameLength);
    }
}