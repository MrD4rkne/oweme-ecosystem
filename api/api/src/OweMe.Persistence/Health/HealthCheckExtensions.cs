using Microsoft.Extensions.DependencyInjection;
using OweMe.Persistence.Ledgers;

namespace OweMe.Persistence.Health;

public static class HealthCheckExtensions
{
    public static IHealthChecksBuilder AddPersistenceHealthCheck(this IHealthChecksBuilder builder)
    {
        builder.AddDbContextCheck<LedgerDbContext>();
        return builder;
    }
}