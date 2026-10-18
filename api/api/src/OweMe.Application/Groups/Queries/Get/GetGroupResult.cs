using OweMe.Application.Common;
using OweMe.Domain.Groups;
using OweMe.Domain.Users;

namespace OweMe.Application.Groups.Queries.Get;

public sealed record GetGroupResult(
    GroupId Id,
    string Name,
    string? Description,
    DateTimeOffset CreatedAt,
    UserId CreatedBy,
    DateTimeOffset? UpdatedAt,
    UserId? UpdatedBy) : AuditableEntityDto(CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
{
    public static GetGroupResult FromDomain(Group group)
    {
        return new GetGroupResult(group.Id, group.Name, group.Description,
            group.CreatedAt, group.CreatedBy, group.UpdatedAt, group.UpdatedBy);
    }
}