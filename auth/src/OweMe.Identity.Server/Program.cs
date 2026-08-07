using Duende.IdentityServer;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OweMe.Identity.Persistence;
using OweMe.Identity.Persistence.Health;
using OweMe.Identity.Server;
using OweMe.Identity.Server.Data;

using var loggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
var bootstrapLogger = loggerFactory.CreateLogger("Startup");

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();

builder.Logging.AddSimpleConsole(options =>
{
    options.IncludeScopes = true;
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss ";
});

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeScopes = true;
    logging.IncludeFormattedMessage = true;
});

var otel = builder.Services.AddOpenTelemetry();
otel.UseOtlpExporter();
otel.WithTracing(b =>
{
    b.AddAspNetCoreInstrumentation(options=>
    {
        options.Filter = context => !context.Request.Path.StartsWithSegments("/.well-known") && !context.Request.Path.StartsWithSegments("/healthz");
    });
    b.AddHttpClientInstrumentation();
    b.AddSource(IdentityServerConstants.Tracing.Basic)
        .AddSource(IdentityServerConstants.Tracing.Cache)
        .AddSource(IdentityServerConstants.Tracing.Services)
        .AddSource(IdentityServerConstants.Tracing.Stores)
        .AddSource(IdentityServerConstants.Tracing.Validation);
}).WithMetrics(b =>
{
    b.AddAspNetCoreInstrumentation();
    b.AddHttpClientInstrumentation();
});

builder.Services.AddHealthChecks()
    .AddPersistenceHealthCheck();

try
{
    builder.AddConnectionStringFromEnv();

    builder.Services.AddOweMeStorage(builder.Configuration.GetConnectionString(Constants.ConnectionStringName));

    builder.AddIdentityServer();

    var app = builder.Build()
        .ConfigurePipeline();

    app.UseHealthChecks("/healthz");

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException && ex.Source != "Microsoft.EntityFrameworkCore.Design") // see https://github.com/dotnet/efcore/issues/29923
{
    bootstrapLogger.LogCritical(ex, "Unhandled exception during application startup");
}
finally
{
    bootstrapLogger.LogInformation("Shut down complete");
}

public abstract partial class Program;
