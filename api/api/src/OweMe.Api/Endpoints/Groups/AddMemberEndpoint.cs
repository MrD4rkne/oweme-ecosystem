using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using OweMe.Api.Description;
using OweMe.Api.Identity;
using OweMe.Application.Common;
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
            .Produces(StatusCodes.Status204NoContent)
            .ProducesExtendedProblem(StatusCodes.Status400BadRequest)
            .ProducesExtendedProblem(StatusCodes.Status404NotFound)
            .ProducesExtendedProblem(StatusCodes.Status403Forbidden)
            .WithStandardProblems()
            .RequireAuthorization(Constants.POLICY_API_SCOPE);
    }

    private static async Task<IResult> AddMember(
        [FromRoute] GroupId groupId,
        [FromBody] AddGroupMemberRequest addMemberRequest,
        IMessageBus messageBus,
        CancellationToken cancellationToken = default)
    {
        var addMemberCommand = new AddGroupMemberCommand(groupId, addMemberRequest.PersonaId);
        var result = await messageBus.InvokeAsync<OneOf<Success, NotFound, Forbidden>>(addMemberCommand, cancellationToken);
        return result.Match(
            _ => Results.NoContent(),
            notFound => notFound.ToProblem(),
            forbidden => forbidden.ToProblem());
    }

    internal sealed record AddGroupMemberRequest(PersonaId PersonaId);
}