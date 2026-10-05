using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OweMe.Application.Expenses;
using OweMe.Domain.Expenses;
using OweMe.Domain.Groups;
using OweMe.Persistence.Groups;

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

        modelBuilder.HasOne<Group>()
            .WithMany()
            .HasForeignKey(expense => expense.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}