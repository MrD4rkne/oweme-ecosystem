using OweMe.Domain.Groups;
using OweMe.Domain.Users;

namespace OweMe.Application.Groups.Commands.AddMember;

public sealed record AddGroupMemberCommand(
    GroupId GroupId,
    PersonaId PersonaId);