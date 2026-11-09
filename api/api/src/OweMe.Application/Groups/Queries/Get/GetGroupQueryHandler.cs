using Microsoft.EntityFrameworkCore;
using OweMe.Application.Common;

namespace OweMe.Application.Groups.Queries.Get;

public static class GetGroupQueryHandler
{
    public static async Task<OneOf<GetGroupResult, NotFound>> HandleAsync(
        GetGroupQuery query,
        IGroupContext context,
        IUserContext userContext,
        CancellationToken cancellationToken)
    {
        var group = await context.Groups.FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);
        if (group is null || !group.CanUserAccess(userContext.Id))
        {
            return new NotFound("Group not found or non accessible.");
        }

        return GetGroupResult.FromDomain(group);
    }
}