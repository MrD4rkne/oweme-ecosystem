using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using OweMe.Api.Description;
using OweMe.Api.Identity;
using OweMe.Application.Groups.Queries.Get;
using Wolverine;

namespace OweMe.Api.Endpoints.Groups.Get;

public sealed class GetGroupByIdEndpoint : IEndpoint
{
    [ExcludeFromCodeCoverage]
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/groups/{groupId:guid}", GetGroup)
            .WithName("GetGroup")
            .WithDescription("Get a group by ID.")
            .WithTags(Tags.Group)
            .Produces<GetGroupResult>()
            .ProducesExtendedProblem(StatusCodes.Status404NotFound)
            .WithStandardProblems()
            .RequireAuthorization(Constants.POLICY_API_SCOPE);
    }

    public static async Task<IResult> GetGroup(
        [FromRoute] Guid groupId,
        IMessageBus messageBus)
    {
        var query = new GetGroupQuery(groupId);
        var group = await messageBus.InvokeAsync<GetGroupResult>(query);
        return Results.Ok(group);
    }
}