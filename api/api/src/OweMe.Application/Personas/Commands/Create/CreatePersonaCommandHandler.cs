using OweMe.Domain.Users;

namespace OweMe.Application.Personas.Commands.Create;

public static class CreatePersonaCommandHandler
{
    public static async Task<PersonaCreated> Handle(CreatePersonaCommand message, IPersonaContext context,
        TimeProvider timeProvider,
        CancellationToken cancellationToken = default)
    {
        var persona = new Persona
        {
            Id = new(Guid.CreateVersion7(timeProvider.GetUtcNow())),
            Name = message.Name,
            UserId = null, // For now, we are not associating the persona with a specific user.
        };

        context.Personas.Add(persona);
        _ = await context.SaveChangesAsync(cancellationToken);
        return new PersonaCreated(persona.Id);
    }

    public sealed record PersonaCreated(PersonaId Id);
}