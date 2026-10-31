using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using OweMe.Persistence.Configuration;
using OweMe.Persistence.Data;
using Shouldly;

namespace OweMe.IntegrationTests;

// Program's entry point builds a host; WebApplicationFactory intercepts host builds process-wide, so run alone.
[CollectionDefinition(DisableParallelization = true)]
public sealed class MigrateCommandCollection;

[Collection(typeof(MigrateCommandCollection))]
public sealed class MigrateCommandShouldApplyMigrations(OweMeApi api)
{
    [Fact]
    public async Task To_AnEmptyDatabase()
    {
        // Arrange
        var connectionString = new NpgsqlConnectionStringBuilder(
            api.Services.GetRequiredService<IOptions<DatabaseOptions>>().Value.ConnectionString)
        {
            Database = "oweme_migrate_command"
        }.ConnectionString;
        await using var factory = api.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.Configure<DatabaseOptions>(options => options.ConnectionString = connectionString)));
        await using var scope = factory.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database;
        (await database.GetPendingMigrationsAsync(TestContext.Current.CancellationToken)).ShouldNotBeEmpty();

        // Act: the same `dotnet OweMe.Api.dll migrate` the deploy runs.
        object? exitCode;
        Environment.SetEnvironmentVariable("Database__ConnectionString", connectionString);
        try
        {
            var main = typeof(OweMe.Api.Program).Assembly.EntryPoint!;
            exitCode = main.Invoke(null, [new[] { "migrate" }]);
        }
        finally
        {
            Environment.SetEnvironmentVariable("Database__ConnectionString", null);
        }

        // Assert
        exitCode.ShouldBe(0);
        (await database.GetPendingMigrationsAsync(TestContext.Current.CancellationToken)).ShouldBeEmpty();
    }
}
