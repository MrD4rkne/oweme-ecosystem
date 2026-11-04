using EntityFramework.Exceptions.Common;
using OweMe.Application.Personas;
using OweMe.Domain.Groups;

namespace OweMe.Application.Groups.Commands.AddMember;

public static class AddGroupMemberCommandHandler
{
    public enum AddGroupMemberResult
    {
        // Start at 1 so default(AddGroupMemberResult) is not a valid outcome.
        GroupNotFound = 1,
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

        var persona = await personaContext.Personas.FindAsync([message.PersonaId], cancellationToken);
        if (persona is null || !persona.CanUserAccess(userContext.Id))
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