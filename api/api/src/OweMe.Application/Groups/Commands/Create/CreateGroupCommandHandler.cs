using OweMe.Domain.Groups;

namespace OweMe.Application.Groups.Commands.Create;

public static class CreateGroupCommandHandler
{
    public static async Task<GroupCreated> Handle(CreateGroupCommand message, IGroupContext context,
        TimeProvider timeProvider,
        CancellationToken cancellationToken = default)
    {
        var group = new Group
        {
            Id = Guid.CreateVersion7(timeProvider.GetUtcNow()),
            Name = message.Name,
            Description = message.Description
        };

        context.Groups.Add(group);
        _ = await context.SaveChangesAsync(cancellationToken);
        return new GroupCreated(
            group.Id
        );
    }

    public sealed record GroupCreated(Guid Id);
}