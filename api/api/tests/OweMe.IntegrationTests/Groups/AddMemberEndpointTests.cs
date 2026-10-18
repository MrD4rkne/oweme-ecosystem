using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OweMe.Api.Endpoints.Groups;
using OweMe.Application.Groups.Commands.Create;
using OweMe.Application.Personas.Commands.Create;
using OweMe.Domain.Groups;
using OweMe.Domain.Users;
using OweMe.IntegrationTests.Authentication;
using OweMe.Persistence.Data;
using Shouldly;

namespace OweMe.IntegrationTests.Groups;

public sealed class AddMemberEndpointTests(OweMeApi api, ITestOutputHelper output)
{
    private readonly WebApplicationFactory<Program> _api = api.WithTestLogging(output);

    [Fact]
    public async Task For_ValidRequest_Should_AddMember()
    {
        // Arrange
        var client = _api.CreateClient().AsUser(new TestUser());
        var groupId = await CreateGroup(client);
        var personaId = await CreatePersona(client);

        // Act
        var response = await client.PostAsJsonAsync($"/api/groups/{groupId}/members",
            new AddMemberEndpoint.AddGroupMemberRequest(personaId),
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await CountMembers(groupId, personaId)).ShouldBe(1);
    }

    [Fact]
    public async Task For_AlreadyAddedMember_Should_ReturnNoContent_AndKeepSingleRow()
    {
        // Arrange
        var client = _api.CreateClient().AsUser(new TestUser());
        var groupId = await CreateGroup(client);
        var personaId = await CreatePersona(client);
        (await client.PostAsJsonAsync($"/api/groups/{groupId}/members",
            new AddMemberEndpoint.AddGroupMemberRequest(personaId),
            TestContext.Current.CancellationToken)).EnsureSuccessStatusCode();

        // Act
        var response = await client.PostAsJsonAsync($"/api/groups/{groupId}/members",
            new AddMemberEndpoint.AddGroupMemberRequest(personaId),
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await CountMembers(groupId, personaId)).ShouldBe(1);
    }

    [Fact]
    public async Task For_NonExistingGroup_Should_ReturnNotFound()
    {
        // Arrange
        var client = _api.CreateClient().AsUser(new TestUser());
        var personaId = await CreatePersona(client);

        // Act
        var response = await client.PostAsJsonAsync($"/api/groups/{Guid.NewGuid()}/members",
            new AddMemberEndpoint.AddGroupMemberRequest(personaId),
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task For_GroupOfAnotherUser_Should_ReturnNotFound()
    {
        // Arrange
        var owner = _api.CreateClient().AsUser(new TestUser());
        var groupId = await CreateGroup(owner);
        var otherUser = _api.CreateClient().AsUser(new TestUser());
        var personaId = await CreatePersona(otherUser);

        // Act
        var response = await otherUser.PostAsJsonAsync($"/api/groups/{groupId}/members",
            new AddMemberEndpoint.AddGroupMemberRequest(personaId),
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await CountMembers(groupId, personaId)).ShouldBe(0);
    }

    [Fact]
    public async Task For_NonExistingPersona_Should_ReturnNotFound()
    {
        // Arrange
        var client = _api.CreateClient().AsUser(new TestUser());
        var groupId = await CreateGroup(client);

        // Act
        var response = await client.PostAsJsonAsync($"/api/groups/{groupId}/members",
            new AddMemberEndpoint.AddGroupMemberRequest(new PersonaId(Guid.NewGuid())),
            TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private static async Task<GroupId> CreateGroup(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/groups", new CreateGroupCommand { Name = "Test Group" },
            TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<CreateGroupCommandHandler.GroupCreated>(
            TestContext.Current.CancellationToken);
        return created.ShouldNotBeNull().Id;
    }

    private static async Task<PersonaId> CreatePersona(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/personas", new CreatePersonaCommand { Name = "Persona" },
            TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<CreatePersonaCommandHandler.PersonaCreated>(
            TestContext.Current.CancellationToken);
        return created.ShouldNotBeNull().Id;
    }

    private async Task<int> CountMembers(GroupId groupId, PersonaId personaId)
    {
        using var scope = _api.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.GroupMembers.CountAsync(m => m.GroupId == groupId && m.PersonaId == personaId,
            TestContext.Current.CancellationToken);
    }
}