using OweMe.Domain.Common;
using OweMe.Domain.Users;

namespace OweMe.Domain.Groups;

public class Group : AuditableEntity
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public bool CanUserAccess(UserId userId)
    {
        return CreatedBy == userId;
    }
}