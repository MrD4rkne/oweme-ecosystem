using OweMe.Domain.Common;
using OweMe.Domain.Groups;

namespace OweMe.Domain.Expenses;

public sealed class Expense : AuditableEntity
{
    public required ExpenseId Id { get; init; }

    public required string Name { get; init; }

    public required GroupId GroupId { get; init; }
}