var builder = DistributedApplication.CreateBuilder(args);

var identityServer = builder.AddProject<Projects.OweMe_Identity_Server>("identityserver");

builder.AddProject<Projects.OweMe_Api>("webapi")
       .WithReference(identityServer)
       .WithEnvironment("IdentityServer__Authority", identityServer.GetEndpoint("https")); 

builder.Build().Run();