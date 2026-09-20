using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using OweMe.IntegrationTests.Authentication;
using Shouldly;

namespace OweMe.IntegrationTests;

public sealed class UserContextShouldBeDerivedFromTheRequest : IClassFixture<OweMeApi>
{
    private readonly DateTimeOffset _currentTime= DateTime.UtcNow;
    private readonly WebApplicationFactory<Program> _api;

    public UserContextShouldBeDerivedFromTheRequest(OweMeApi api, ITestOutputHelper output)
    {
        _api = api.WithTestLogging(output)
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var timeProvider = new FakeTimeProvider();
                    timeProvider.SetUtcNow(_currentTime);
                    services.AddSingleton<TimeProvider>(timeProvider);
                });
            });
    }
    
    [Fact]
    public async Task CreatedX_Properties_ShouldBeDerivedFromTheRequest()
    {
        // Arrange
        var user = new TestUser();
        var client = _api.CreateClient()
            .AsUser(user);

        var ledger = new Application.Ledgers.Commands.Create.CreateLedgerCommand()
        {
            Name = "Test Ledger",
            Description = "Test Description"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/ledgers", ledger, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        
        // Assert
        var ledgerLocation = response.Headers.Location;
        
        var createdLedger = await client.GetFromJsonAsync<Application.Ledgers.Queries.Get.GetLedgerResult>(ledgerLocation, TestContext.Current.CancellationToken);

        createdLedger.ShouldNotBeNull();
        createdLedger.CreatedBy.ShouldBe(user.Id);
       
        // CreatedBy might lose precision when stored in the database, so we allow a small tolerance for the CreatedAt comparison.
        var tolerance = TimeSpan.FromSeconds(1);
        createdLedger.CreatedAt.ShouldBe(_currentTime, tolerance);
        
        createdLedger.UpdatedBy.ShouldBeNull();
        createdLedger.UpdatedAt.ShouldBeNull();
    }
}