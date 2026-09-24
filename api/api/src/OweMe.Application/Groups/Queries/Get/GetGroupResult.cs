using OweMe.Application.Common;
using OweMe.Domain.Groups;

namespace OweMe.Application.Groups.Queries.Get;

public sealed record GetGroupResult(
    Guid Id,
    string Name,
    string? Description,
    DateTimeOffset CreatedAt,
    Guid CreatedBy,
    DateTimeOffset? UpdatedAt,
    Guid? UpdatedBy) : AuditableEntityDto(CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
{
    public static GetGroupResult FromDomain(Group group)
    {
        return new GetGroupResult(group.Id, group.Name, group.Description,
            group.CreatedAt, group.CreatedBy.Id, group.UpdatedAt, group.UpdatedBy?.Id);
    }
}