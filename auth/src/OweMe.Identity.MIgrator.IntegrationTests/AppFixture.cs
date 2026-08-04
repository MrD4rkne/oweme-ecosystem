using System.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using Xunit.Abstractions;

namespace OweMe.Identity.Migrator.IntegrationTests;

public sealed class AppFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _databaseContainer = new PostgreSqlBuilder()
        .WithDatabase("testdb")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public string ConnectionString => _databaseContainer.GetConnectionString();

    public RootCommand Build(ITestOutputHelper testOutputHelper)
    {
        return App.BuildRootCommand((services, _) =>
        {
            services.AddLogging(logging => logging.AddXUnit(testOutputHelper));
        },
        configBuilder =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ConnectionString
            });
        });
    }

    public Task InitializeAsync() => _databaseContainer.StartAsync();

    public Task DisposeAsync() => _databaseContainer.DisposeAsync().AsTask();
}
