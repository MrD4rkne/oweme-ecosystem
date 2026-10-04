using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OweMe.Application.Groups;
using OweMe.Persistence.Configuration;
using OweMe.Persistence.Data;

namespace OweMe.Persistence;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddPersistence(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddOptions<DatabaseOptions>()
            .Bind(builder.Configuration.GetSection(DatabaseOptions.SectionName))
            .ValidateOnStart();

        builder.Services.AddOweMeDbContext<ApplicationDbContext>();

        builder.Services.AddScoped<IGroupContext, ApplicationDbContext>();
        builder.Services.AddHostedService<MigrationHostedService>();

        return builder;
    }

    private static IServiceCollection AddOweMeDbContext<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        services.AddDbContext<TContext>((serviceProvider, options) =>
        {
            var dbOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            options.UseNpgsql(dbOptions.ConnectionString);
            options.EnableSensitiveDataLogging();
        });
        return services;
    }
}