using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using OweMe.Api;
using OweMe.Api.Identity.Configuration;
using OweMe.IntegrationTests.Authentication;
using OweMe.Persistence.Configuration;
using Testcontainers.PostgreSql;

namespace OweMe.IntegrationTests;

public sealed class OweMeApi : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithDatabase("oweme_test")
        .WithPortBinding(5432, assignRandomHostPort: true)
        .Build();

    private string ConnectionString => _postgresContainer.GetConnectionString();

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

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.Configure<DatabaseOptions>(options =>
            {
                options.ConnectionString = ConnectionString;
                options.RunMigrations = true;
            });

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