using OweMe.Domain.Users;

namespace OweMe.Domain.Groups;

public sealed class GroupMember
{
    public required GroupId GroupId { get; init; }

    public required PersonaId PersonaId { get; init; }
}