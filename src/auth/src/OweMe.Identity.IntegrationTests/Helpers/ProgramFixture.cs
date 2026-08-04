using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OweMe.Identity.Server.Data;
using Testcontainers.PostgreSql;
using Xunit.Abstractions;

public class ProgramFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _databaseContainer = new PostgreSqlBuilder()
        .WithDatabase("testdb")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithPortBinding(5432, true)
        .Build();

    protected ITestOutputHelper? TestOutputHelper { get; private set; }

    public ProgramFixture WithTestOutputHelper(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
        return this;
    }

    protected string ConnectionString => _databaseContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _databaseContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _databaseContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connectionString = _databaseContainer.GetConnectionString();
        builder.UseSetting($"ConnectionStrings:{Constants.ConnectionStringName}", connectionString);

        builder.ConfigureServices(services =>
        {
            DisableTokenCleanup(services);

            services.AddLogging(logging =>
            {
                if (TestOutputHelper != null)
                {
                    logging.AddXUnit(TestOutputHelper);
                }
            });
        });
    }

    /// <summary>
    /// Disable Duende's token clean up.
    /// </summary>
    /// <remarks>
    /// Disabling by overriding configuration lead to failures
    /// during Teardown.
    /// </remarks>
    private static void DisableTokenCleanup(IServiceCollection services)
    {
        var cleanupServices = services
            .Where(d => d.ServiceType == typeof(IHostedService) &&
                        d.ImplementationType?.Name == "TokenCleanupHost")
            .ToList();

        foreach (var descriptor in cleanupServices)
        {
            services.Remove(descriptor);
        }
    }
}
