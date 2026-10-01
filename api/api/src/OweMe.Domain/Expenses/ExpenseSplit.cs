namespace OweMe.Domain.Expenses;

public sealed class ExpenseSplit
{
   public required double Amount { get; init; }
   
   public required Guid ExpenseId { get; init; }
   
   public required Guid PersonaId { get; init; }
}