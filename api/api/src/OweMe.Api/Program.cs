using JasperFx;
using JasperFx.CodeGeneration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using OweMe.Api.Configuration;
using OweMe.Api.Description;
using OweMe.Api.Endpoints;
using OweMe.Api.Identity;
using OweMe.Api.Identity.Configuration;
using OweMe.Api.Identity.Description;
using OweMe.Api.User;
using OweMe.Application;
using OweMe.Application.Common.Middlewares;
using OweMe.Application.Groups;
using OweMe.Application.User;
using OweMe.Infrastructure;
using OweMe.Persistence;
using OweMe.Persistence.Health;
using OweMe.ServiceDefaults;
using Scalar.AspNetCore;
using Wolverine;
using Wolverine.FluentValidation;
using DependencyInjection = OweMe.Application.User.DependencyInjection;

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

builder.AddServiceDefaults();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<OAuth2SecuritySchemeTransformer>();
    options.AddDocumentTransformer<ApiVersionOpenApiDocumentTransformer>();
});

var identityOptions = builder.Services.AddOptions<IdentityServerOptions>()
    .Bind(builder.Configuration.GetSection(IdentityServerOptions.SectionName));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IApiInformationProvider, ApiInformationProvider>();

builder.Services.ConfigureOptions<ConfigureJwtBearerOptions>();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Constants.POLICY_API_SCOPE, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            context.User.FindAll("scope")
                .SelectMany(c => c.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .Contains(Constants.POLICY_API_SCOPE_CLAIM));
    });

builder.AddApplication();
builder.AddInfrastructure();
builder.AddPersistence();

builder.UseWolverine(opts =>
{
    opts.Discovery.IncludeAssembly(typeof(DependencyInjection).Assembly);
    opts.CodeGeneration.AlwaysUseServiceLocationFor<IGroupContext>();
    opts.CodeGeneration.AlwaysUseServiceLocationFor<IUserContext>();
    opts.CodeGeneration.AlwaysUseServiceLocationFor<IUserContextSetter>();

    opts.Policies.AddMiddleware<PerformanceMiddleware>();
    opts.Policies.AddMiddleware(typeof(UserContextWolverineMiddleware));

    opts.UseFluentValidation(RegistrationBehavior.ExplicitRegistration);

    if (builder.Environment.IsProduction() && !CodeGeneration.IsRunningGeneration())
    {
        opts.CodeGeneration.TypeLoadMode = TypeLoadMode.Static;
        opts.Services.CritterStackDefaults(cr => { cr.Production.AssertAllPreGeneratedTypesExist = true; });
    }
    else
    {
        // Fallback to Auto for local development/debugging
        opts.CodeGeneration.TypeLoadMode = TypeLoadMode.Dynamic;
    }
});

builder.Services.AddExceptionHandler<ExceptionProblemDetailsMatcher>();

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails = new ExtendedProblemDetails(context.ProblemDetails)
        {
            Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}",
            RequestId = context.HttpContext.TraceIdentifier,
            TraceId = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity?.Id
        };
    };
});

builder.Services.AddEndpoints(typeof(OweMe.Api.Program).Assembly);

if (!CodeGeneration.IsRunningGeneration())
    // Some actions like validating application options must not be run during codegen activities, like OpenApi spec
    // generation or managing Entity Framework Core migrations.
    identityOptions
        .ValidateDataAnnotations()
        .ValidateOnStart();

builder.Services.AddHealthChecks()
    .AddPersistenceHealthCheck();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(opt =>
    {
        opt.AddPreferredSecuritySchemes("OAuth2")
            .AddPasswordFlow("OAuth2", flow => { flow.SelectedScopes = [Constants.POLICY_API_SCOPE_CLAIM]; });
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapDefaultEndpoints();

return await app.RunJasperFxCommands(args);

namespace OweMe.Api
{
    public class Program
    {
        protected Program()
        {
        }
    }
}