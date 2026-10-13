using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OweMe.Application.Personas.Commands.Create;
using OweMe.Domain.Users;
using OweMe.IntegrationTests.Authentication;
using OweMe.Persistence.Data;
using Shouldly;

namespace OweMe.IntegrationTests.Personas;

public sealed class CreatePersonaEndpointTests(OweMeApi api, ITestOutputHelper output)
{
    private readonly WebApplicationFactory<Program> _api = api.WithTestLogging(output);

    [Fact]
    public async Task For_ValidRequest_Should_CreatePersona()
    {
        // Arrange
        var user = new TestUser();
        var client = _api.CreateClient().AsUser(user);
        var command = new CreatePersonaCommand { Name = "SomeNewPersonaName" };

        // Act
        var response = await client.PostAsJsonAsync("/api/personas", command, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
        var id = Guid.Parse(response.Headers.Location.OriginalString.Replace("/api/personas/", ""));
        id.Version.ShouldBe(7);

        // No GET endpoint for personas yet, so check the database directly.
        using var scope = _api.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var persona = await db.Personas.SingleOrDefaultAsync(x => x.Id == new PersonaId(id),
            TestContext.Current.CancellationToken);
        persona.ShouldNotBeNull();
        persona.Name.ShouldBe(command.Name);
        persona.CreatedBy.ShouldBe(new UserId(user.Id));
    }

    [Fact]
    public async Task For_InvalidRequest_Should_ReturnBadRequest()
    {
        // Arrange
        var client = _api.CreateClient().AsUser(new TestUser());
        var command = new CreatePersonaCommand { Name = "" };

        // Act
        var response = await client.PostAsJsonAsync("/api/personas", command, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}