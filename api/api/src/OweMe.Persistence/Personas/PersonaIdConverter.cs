using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OweMe.Domain.Users;

namespace OweMe.Persistence.Personas;

public sealed class PersonaIdConverter()
    : ValueConverter<PersonaId, Guid>(id => id.Value, v => new PersonaId(v));