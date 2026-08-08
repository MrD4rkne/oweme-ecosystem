using System.Diagnostics.CodeAnalysis;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Microsoft.Extensions.DependencyInjection;
using OweMe.Identity.Persistence.IdentityServer;
using OweMe.Identity.Persistence.Users;

namespace OweMe.Identity.Persistence.Health;

[ExcludeFromCodeCoverage]
public static class HealthCheckExtensions
{
    public static IHealthChecksBuilder AddPersistenceHealthCheck(this IHealthChecksBuilder builder)
    {
        builder.AddDbContextCheck<ApplicationDbContext>();
        builder.AddDbContextCheck<DataProtectionDbContext>();
        builder.AddDbContextCheck<ConfigurationDbContext>();
        builder.AddDbContextCheck<PersistedGrantDbContext>();
        return builder;
    }
}
