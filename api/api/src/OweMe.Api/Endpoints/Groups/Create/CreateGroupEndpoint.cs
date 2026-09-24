using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using OweMe.Api.Description;
using OweMe.Api.Identity;
using OweMe.Application.Groups.Commands.Create;
using Wolverine;

namespace OweMe.Api.Endpoints.Groups.Create;

public sealed class CreateGroupEndpoint : IEndpoint
{
    [ExcludeFromCodeCoverage]
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/groups", CreateGroup)
            .WithName("CreateGroup")
            .WithDescription("Create a new group that groups expenses and payments between users.")
            .WithTags(Tags.Group)
            .Produces(StatusCodes.Status201Created)
            .ProducesExtendedProblem(StatusCodes.Status400BadRequest)
            .WithStandardProblems()
            .RequireAuthorization(Constants.POLICY_API_SCOPE);
    }

    public static async Task<IResult> CreateGroup(
        [FromBody] CreateGroupCommand createGroupRequest,
        IMessageBus messageBus,
        CancellationToken cancellationToken = default)
    {
        var createGroupCommand = new CreateGroupCommand
        {
            Name = createGroupRequest.Name,
            Description = createGroupRequest.Description
        };

        var group =
            await messageBus.InvokeAsync<CreateGroupCommandHandler.GroupCreated>(createGroupCommand,
                cancellationToken);
        return Results.Created($"/api/groups/{group.Id}", null);
    }
}