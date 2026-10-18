using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using OweMe.Api.Description;
using OweMe.Api.Identity;
using OweMe.Application.Groups.Commands.AddMember;
using OweMe.Domain.Groups;
using OweMe.Domain.Users;
using Wolverine;

namespace OweMe.Api.Endpoints.Groups;

internal sealed class AddMemberEndpoint : IEndpoint
{
    [ExcludeFromCodeCoverage]
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/groups/{groupId:guid}/members", AddMember)
            .WithName("AddMember")
            .WithDescription("Add a member to a group.")
            .WithTags(Tags.Group)
            .Produces(StatusCodes.Status200OK)
            .ProducesExtendedProblem(StatusCodes.Status400BadRequest)
            .ProducesExtendedProblem(StatusCodes.Status404NotFound)
            .WithStandardProblems()
            .RequireAuthorization(Constants.POLICY_API_SCOPE);
    }

    private static async Task<IResult> AddMember(
        [FromRoute] GroupId groupId,
        [FromBody] AddGroupMemberRequest addMemberRequest,
        IMessageBus messageBus)
    {
        var addMemberCommand = new AddGroupMemberCommand(groupId, addMemberRequest.PersonaId);
        return await messageBus.InvokeAsync<AddGroupMemberCommandHandler.AddGroupMemberResult>(addMemberCommand) switch
        {
            AddGroupMemberCommandHandler.AddGroupMemberResult.GroupNotFound => Results.NotFound(new
                { Message = "Group not found or non accessible." }),
            AddGroupMemberCommandHandler.AddGroupMemberResult.UserNotAuthorized => Results.Forbid(),
            AddGroupMemberCommandHandler.AddGroupMemberResult.PersonaNotFound => Results.NotFound(new
                { Message = "Persona not found." }),
            AddGroupMemberCommandHandler.AddGroupMemberResult.MemberAdded => Results.NoContent(),
            _ => throw new ArgumentOutOfRangeException(nameof(addMemberCommand),
                "Unexpected result for adding member to group.")
        };
    }

    internal sealed record AddGroupMemberRequest(PersonaId PersonaId);
}