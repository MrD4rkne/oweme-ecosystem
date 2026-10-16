using Microsoft.Extensions.DependencyInjection;
using OweMe.Persistence.Data;

namespace OweMe.Persistence.Health;

public static class HealthCheckExtensions
{
    public static IHealthChecksBuilder AddPersistenceHealthCheck(this IHealthChecksBuilder builder)
    {
        builder.AddDbContextCheck<ApplicationDbContext>();
        return builder;
    }
}