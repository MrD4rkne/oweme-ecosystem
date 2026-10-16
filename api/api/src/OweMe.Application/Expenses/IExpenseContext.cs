using Microsoft.EntityFrameworkCore;
using OweMe.Domain.Expenses;

namespace OweMe.Application.Expenses;

public interface IExpensesContext
{
    DbSet<Expense> Expenses { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}