namespace OweMe.Application.Personas.Commands.Create;

public sealed record CreatePersonaCommand
{
    public required string Name { get; init; }
}