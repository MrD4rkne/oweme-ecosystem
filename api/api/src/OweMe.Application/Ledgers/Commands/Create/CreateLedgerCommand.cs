namespace OweMe.Application.Groups.Commands.Create;

public record CreateGroupCommand
{
    public required string Name { get; init; }

    public string? Description { get; init; }
}