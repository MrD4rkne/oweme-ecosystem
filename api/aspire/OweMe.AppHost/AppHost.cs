using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var apiDbServer = builder.AddPostgres("postgres-api")
    .WithDataVolume();

var apiDb = apiDbServer.AddDatabase("api-database", "OweMe.Api");

var keycloakDbServer = builder.AddPostgres("postgres-keycloak")
    .WithDataVolume();

var keycloakDb = keycloakDbServer.AddDatabase("keycloak-db");

var keycloak = builder.AddKeycloak("keycloak", 8080)
    .WithPostgres(keycloakDb)
    .WithRealmImport("../../../auth/realms")
    .WithEnvironment("KC_HOSTNAME_STRICT", "false")
    .WithEnvironment("KC_CACHE", "local") // speed up local spin up
    .WithEnvironment("KC_FEATURES", "token-exchange")
    .WithDataVolume()
    .WithOtlpExporter();

// No launch profile: the migrator is a one-shot `migrate` command and must not bind the api's ports.
var apiMigrator = builder.AddProject<OweMe_Api>("api-migrator", launchProfileName: null)
    .WithArgs("migrate")
    .WithReference(apiDb)
    .WaitFor(apiDb)
    .WithEnvironment("Database__ConnectionString", apiDb);

_ = builder.AddProject<OweMe_Api>("api")
    .WithHttpEndpoint(5000, name: "http")
    .WithReference(apiDb)
    .WithReference(keycloak)
    .WaitForCompletion(apiMigrator)
    .WaitFor(keycloak)
    .WithEnvironment("IdentityServer__Authority", $"{keycloak.GetEndpoint("http")}/realms/oweme-dev")
    .WithEnvironment("IdentityServer__MetadataAddress",
        $"{keycloak.GetEndpoint("http")}/realms/oweme-dev/.well-known/openid-configuration")
    .WithEnvironment("IdentityServer__ValidateAudience", "false")
    .WithEnvironment("IdentityServer__RequireHttpsMetadata", "false")
    .WithEnvironment("Database__ConnectionString", apiDb);

await using var app = builder.Build();
await app.RunAsync();