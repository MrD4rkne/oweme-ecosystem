using OweMe.Domain.Groups;

namespace OweMe.Application.Groups.Commands.Create;

public static class CreateGroupCommandHandler
{
    public static async Task<GroupCreated> Handle(CreateGroupCommand message, IGroupContext context,
        CancellationToken cancellationToken = default)
    {
        var group = new Group
        {
            Name = message.Name,
            Description = message.Description
        };

        context.Ledgers.Add(group);
        _ = await context.SaveChangesAsync(cancellationToken);
        return new GroupCreated(
            group.Id
        );
    }

    public sealed record GroupCreated(Guid Id);
}