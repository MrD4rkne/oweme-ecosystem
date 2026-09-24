using Microsoft.EntityFrameworkCore;
using OweMe.Application.Common.Exceptions;

namespace OweMe.Application.Groups.Queries.Get;

public static class GetGroupQueryHandler
{
    public static async Task<GetGroupResult> HandleAsync(
        GetGroupQuery query,
        IGroupContext context,
        IUserContext userContext,
        CancellationToken cancellationToken)
    {
        var group = await context.Groups.FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);
        if (group is null || !group.CanUserAccess(userContext.Id))
        {
            throw new NotFoundException($"Group with id {query.Id} not found.");
        }

        return GetGroupResult.FromDomain(group);
    }
}