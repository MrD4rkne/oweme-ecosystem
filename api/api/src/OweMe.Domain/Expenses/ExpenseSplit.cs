using OweMe.Domain.Users;

namespace OweMe.Domain.Expenses;

public sealed class ExpenseSplit
{
    public required double Amount { get; init; }

    public required ExpenseId ExpenseId { get; init; }

    public required PersonaId PersonaId { get; init; }
}