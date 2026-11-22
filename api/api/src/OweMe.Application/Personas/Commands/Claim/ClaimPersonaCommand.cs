using OweMe.Domain.Users;

namespace OweMe.Application.Personas.Commands.Claim;

public sealed record ClaimPersonaCommand
{
    public required PersonaId PersonaId { get; init; }
    public required UserId UserToBeAssigned { get; init; }
}