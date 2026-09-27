using OweMe.Domain.Common;
using OweMe.Domain.Users;

namespace OweMe.Domain.Groups;

public sealed class Group : AuditableEntity
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public string? Description { get; init; }

    public bool CanUserAccess(UserId userId)
    {
        return CreatedBy == userId;
    }
}