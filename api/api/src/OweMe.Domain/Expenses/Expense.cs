using OweMe.Domain.Common;

namespace OweMe.Domain.Expenses;

public sealed class Expense : AuditableEntity
{
    public required ExpenseId Id { get; init; }

    public required string Name { get; init; }

    public required Guid GroupId { get; init; }
}