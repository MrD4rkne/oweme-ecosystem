using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OweMe.Domain.Expenses;

namespace OweMe.Persistence.Expenses;

public sealed class ExpenseIdConverter()
    : ValueConverter<ExpenseId, Guid>(id => id.Value, v => new ExpenseId(v));