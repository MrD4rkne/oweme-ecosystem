using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using OweMe.Persistence.Configuration;
using Testcontainers.PostgreSql;
using Xunit.Internal;

namespace OweMe.IntegrationTests;

public class OweMeApi : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithDatabase("oweme_test")
        .WithPortBinding(5432, assignRandomHostPort: true)
        .Build();

    public string ConnectionString => _postgresContainer.GetConnectionString();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.Configure<DatabaseOptions>(options =>
            {
                options.ConnectionString = ConnectionString;
                options.RunMigrations = true;
            });
        });
    }

    public async ValueTask InitializeAsync()
    {
        await _postgresContainer.StartAsync();
    }
    
    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();

        await _postgresContainer.StopAsync();
        await _postgresContainer.DisposeAsync();

        GC.SuppressFinalize(this);
    }
}