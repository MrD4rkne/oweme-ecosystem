using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;

namespace OweMe.IntegrationTests;

public static class LoggingExtensions
{
    /// <summary>
    /// Redirect all application logging to the test console.
    /// </summary>
    public static WebApplicationFactory<TStartup> WithTestLogging<TStartup>(this WebApplicationFactory<TStartup> factory, ITestOutputHelper output) where TStartup : class
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddXUnit(outputHelper: output);
            });
        });
    }
}