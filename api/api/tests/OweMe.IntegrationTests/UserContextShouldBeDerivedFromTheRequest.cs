using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using OweMe.Api;
using OweMe.Application.Groups.Commands.Create;
using OweMe.Application.Groups.Queries.Get;
using OweMe.IntegrationTests.Authentication;
using Shouldly;

namespace OweMe.IntegrationTests;

public sealed class UserContextShouldBeDerivedFromTheRequest : IClassFixture<OweMeApi>
{
    private readonly WebApplicationFactory<Program> _api;
    private readonly DateTimeOffset _currentTime = DateTime.UtcNow;

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

        var group = new CreateGroupCommand()
        {
            Name = "Test Group",
            Description = "Test Description"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/groups", group, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        // Assert
        var groupLocation = response.Headers.Location;

        var createdGroup =
            await client.GetFromJsonAsync<GetGroupResult>(groupLocation, TestContext.Current.CancellationToken);

        createdGroup.ShouldNotBeNull();
        createdGroup.CreatedBy.ShouldBe(user.Id);

        // CreatedBy might lose precision when stored in the database, so we allow a small tolerance for the CreatedAt comparison.
        var tolerance = TimeSpan.FromSeconds(1);
        createdGroup.CreatedAt.ShouldBe(_currentTime, tolerance);

        createdGroup.UpdatedBy.ShouldBeNull();
        createdGroup.UpdatedAt.ShouldBeNull();
    }
}