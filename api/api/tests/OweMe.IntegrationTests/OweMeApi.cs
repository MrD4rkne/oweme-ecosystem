using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using OweMe.Api.Identity.Configuration;
using OweMe.IntegrationTests.Authentication;
using Microsoft.EntityFrameworkCore;
using OweMe.Persistence.Configuration;
using OweMe.Persistence.Data;
using Testcontainers.PostgreSql;

namespace OweMe.IntegrationTests;

public sealed class OweMeApi : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder("postgres:15.3")
        .WithDatabase("oweme_test")
        .WithPortBinding(5432, true)
        .Build();

    private string ConnectionString => _postgresContainer.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        // Migrate once before tests create their own derived hosts in parallel.
        await using var scope = Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync(TestContext.Current.CancellationToken);
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();

        await _postgresContainer.StopAsync();
        await _postgresContainer.DisposeAsync();

        GC.SuppressFinalize(this);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.Configure<DatabaseOptions>(options => options.ConnectionString = ConnectionString);

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, null);
        });

        builder.ConfigureTestServices(services =>
        {
            services.PostConfigure<IdentityServerOptions>(options =>
            {
                options.Authority = "https://mock-keycloak/realms/testrealm";
            });
        });
    }
}