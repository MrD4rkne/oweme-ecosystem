using Microsoft.EntityFrameworkCore;

namespace OweMe.Application.Groups.Queries.Get;

public static class GetGroupQueryHandler
{
    // null means the group does not exist or the user cannot access it.
    public static async Task<GetGroupResult?> HandleAsync(
        GetGroupQuery query,
        IGroupContext context,
        IUserContext userContext,
        CancellationToken cancellationToken)
    {
        var group = await context.Groups.FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);
        if (group is null || !group.CanUserAccess(userContext.Id))
        {
            return null;
        }

        return GetGroupResult.FromDomain(group);
    }
}