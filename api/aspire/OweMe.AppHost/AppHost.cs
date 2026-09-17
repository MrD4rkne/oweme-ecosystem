var builder = DistributedApplication.CreateBuilder(args);

var apiDbServer = builder.AddPostgres("postgres-api")
    .WithDataVolume();

var apiDb = apiDbServer.AddDatabase("api-database", databaseName: "OweMe.Api");

var keycloakDbServer = builder.AddPostgres("postgres-keycloak")
    .WithDataVolume();
                              
var keycloakDb = keycloakDbServer.AddDatabase("keycloak-db");

var keycloak = builder.AddKeycloak("keycloak", port: 8080)
    .WithPostgres(keycloakDb)
    .WithRealmImport("../../../auth/realms")
    .WithEnvironment("KC_HOSTNAME_STRICT", "false")
    .WithEnvironment("KC_CACHE", "local") // speed up local spin up
    .WithEnvironment("KC_FEATURES", "token-exchange")
    .WithDataVolume()
    .WithOtlpExporter();

var api = builder.AddProject<Projects.OweMe_Api>("api")
    .WithHttpEndpoint(port: 5000, name: "http")
    .WithReference(apiDb)
    .WithReference(keycloak)
    .WaitFor(apiDb)
    .WaitFor(keycloak)
    .WithEnvironment("IdentityServer__Authority", $"{keycloak.GetEndpoint("http")}/realms/oweme-dev")
    .WithEnvironment("IdentityServer__MetadataAddress",
        $"{keycloak.GetEndpoint("http")}/realms/oweme-dev/.well-known/openid-configuration")
    .WithEnvironment("IdentityServer__ValidateAudience", "false")
    .WithEnvironment("IdentityServer__RequireHttpsMetadata", "false")
    .WithEnvironment("Database__RunMigrations", "true")
    .WithEnvironment("Database__ConnectionString", apiDb);

await using var app = builder.Build();
await app.RunAsync();
