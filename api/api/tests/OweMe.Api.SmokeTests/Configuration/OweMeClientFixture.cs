using Duende.IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OweMe.Api.Client;
using OweMe.Api.SmokeTests.Http;

namespace OweMe.Api.SmokeTests.Configuration;

public class OweMeClientFixture
{
    public const string AuthenticatedClientKey = "Authenticated";
    public const string UnauthenticatedClientKey = "Unauthenticated";

    private readonly IServiceProvider _serviceProvider;

    public OweMeClientFixture()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddXUnit());

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", true)
            .AddEnvironmentVariables()
            .Build();

        services.Configure<ApiSettings>(configuration.GetSection(ApiSettings.SectionName));
        services.Configure<UserSettings>(configuration.GetSection(UserSettings.SectionName));
        services.Configure<IdentityProviderSettings>(configuration.GetSection(IdentityProviderSettings.SectionName));

        services.AddMemoryCache();
        services.AddSingleton<IDiscoveryCache>(r =>
        {
            var factory = r.GetRequiredService<IHttpClientFactory>();
            var discoverySettings = r.GetRequiredService<IOptions<IdentityProviderSettings>>().Value;
            return new DiscoveryCache(discoverySettings.Address, factory.CreateClient);
        });

        services.AddHttpClient<TokenManager>()
            .AddHttpMessageHandler<LoggingDelegatingHandler>();
        services.AddSingleton<ITokenManager>(sp => sp.GetRequiredService<TokenManager>());

        services.AddHttpClient(AuthenticatedClientKey, (sp, client) =>
            {
                var testSettings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
                client.BaseAddress = new Uri(testSettings.BaseUrl);
            }).AddHttpMessageHandler<LoggingDelegatingHandler>()
            .AddHttpMessageHandler<AuthorizationDelegatingHandler>();

        services.AddKeyedTransient(AuthenticatedClientKey, CreateOweMeClientFromKey);

        services.AddHttpClient(UnauthenticatedClientKey, (sp, client) =>
        {
            var testSettings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
            client.BaseAddress = new Uri(testSettings.BaseUrl);
        }).AddHttpMessageHandler<LoggingDelegatingHandler>();

        services.AddKeyedTransient(UnauthenticatedClientKey, CreateOweMeClientFromKey);

        services.AddTransient<LoggingDelegatingHandler>();
        services.AddTransient<AuthorizationDelegatingHandler>();

        _serviceProvider = services.BuildServiceProvider();
    }

    private static OweMeApiClient CreateOweMeClientFromKey(IServiceProvider sp, object? key)
    {
        var name = key?.ToString();
        ArgumentNullException.ThrowIfNullOrEmpty(name);

        var clientFactory = sp.GetRequiredService<IHttpClientFactory>();
        return new OweMeApiClient(clientFactory.CreateClient(name));
    }

    public OweMeApiClient GetClient(string clientKey)
    {
        return _serviceProvider.GetRequiredKeyedService<OweMeApiClient>(clientKey) ??
               throw new InvalidOperationException($"Client with key '{clientKey}' not found.");
    }
}