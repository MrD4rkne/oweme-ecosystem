using EntityFramework.Exceptions.Common;
using OweMe.Application.Common;
using OweMe.Application.Personas;
using OweMe.Domain.Groups;

namespace OweMe.Application.Groups.Commands.AddMember;

public static class AddGroupMemberCommandHandler
{
    public static async Task<OneOf<Success, NotFound>> Handle(AddGroupMemberCommand message,
        IGroupContext context,
        IPersonaContext personaContext,
        IUserContext userContext,
        CancellationToken cancellationToken = default)
    {
        var group = await context.Groups.FindAsync([message.GroupId], cancellationToken);
        if (group is null || !group.CanUserAccess(userContext.Id))
        {
            return new NotFound("Group not found or non accessible.");
        }

        var persona = await personaContext.Personas.FindAsync([message.PersonaId], cancellationToken);
        if (persona is null || !persona.CanUserAccess(userContext.Id))
        {
            return new NotFound("Persona not found.");
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
            return new Success();
        }

        return new Success();
    }
}