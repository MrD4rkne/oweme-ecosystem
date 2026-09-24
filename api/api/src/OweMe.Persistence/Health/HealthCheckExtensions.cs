using Microsoft.Extensions.DependencyInjection;
using OweMe.Persistence.Groups;

namespace OweMe.Persistence.Health;

public static class HealthCheckExtensions
{
    public static IHealthChecksBuilder AddPersistenceHealthCheck(this IHealthChecksBuilder builder)
    {
        builder.AddDbContextCheck<GroupDbContext>();
        return builder;
    }
}