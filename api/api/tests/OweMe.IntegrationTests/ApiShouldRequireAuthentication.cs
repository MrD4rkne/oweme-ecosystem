using Microsoft.AspNetCore.Mvc.Testing;
using OweMe.Application.Ledgers.Commands.Create;
using OweMe.IntegrationTests.Authentication;
using Shouldly;

namespace OweMe.IntegrationTests;

public sealed class ApiShouldRequireAuthentication(OweMeApi api, ITestOutputHelper output) : IClassFixture<OweMeApi>
{
    private readonly WebApplicationFactory<Program> _api = api.WithTestLogging(output);

    private async Task VerifyEndpointRequiresAuthentication(Func<HttpClient, Task<HttpResponseMessage>> requestFunc)
    {
        // Arrange
        var client = _api.CreateClient()
            .AsAnonymous();

        // Act
        var response = await requestFunc(client);

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task For_GetLedgersByIdEndpoint()
    {
        await VerifyEndpointRequiresAuthentication(client => client.GetAsync($"/api/ledgers/{Guid.NewGuid()}"));
    }
    
    [Fact]
    public async Task For_CreateLedgerEndpoint()
    {
        var ledger = new CreateLedgerCommand()
        {
            Name = "Test Ledger",
            Description = "Test Description"
        };
        
        await VerifyEndpointRequiresAuthentication(client => client.PostAsync("/api/ledgers", new StringContent(System.Text.Json.JsonSerializer.Serialize(ledger), System.Text.Encoding.UTF8, "application/json")));
    }
}