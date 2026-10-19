using EntityFramework.Exceptions.Common;
using Microsoft.EntityFrameworkCore;
using OweMe.Application.Personas;
using OweMe.Domain.Groups;

namespace OweMe.Application.Groups.Commands.AddMember;

public static class AddGroupMemberCommandHandler
{
    public enum AddGroupMemberResult
    {
        GroupNotFound,
        UserNotAuthorized,
        PersonaNotFound,
        MemberAdded,
    }

    public static async Task<AddGroupMemberResult> Handle(AddGroupMemberCommand message,
        IGroupContext context,
        IPersonaContext personaContext,
        IUserContext userContext,
        CancellationToken cancellationToken = default)
    {
        var group = await context.Groups.FindAsync([message.GroupId], cancellationToken);
        if (group is null || !group.CanUserAccess(userContext.Id))
        {
            return AddGroupMemberResult.GroupNotFound;
        }

        if (!group.CanUserEdit(userContext.Id))
        {
            return AddGroupMemberResult.UserNotAuthorized;
        }

        if (!await personaContext.Personas.AnyAsync(x => x.Id == message.PersonaId,
                cancellationToken: cancellationToken))
        {
            return AddGroupMemberResult.PersonaNotFound;
        }

        var member = new GroupMember
        {
            GroupId = message.GroupId,
            PersonaId = message.PersonaId
        };
        context.GroupMembers.Add(member);

        try
        {
            _ = await context.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintException)
        {
            // The member is already in the group; adding is idempotent.
            return AddGroupMemberResult.MemberAdded;
        }

        return AddGroupMemberResult.MemberAdded;
    }
}