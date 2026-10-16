using OweMe.Domain.Common;

namespace OweMe.Domain.Users;

/// <summary>
/// Represents a persona in the system.
/// </summary>
/// <remarks>
/// Persona allows to:
/// <list type="bullet">
/// <item> Have multiple personas for a single user. </item>
/// <item> Add expenses for a persona that is not the user themselves (e.g., a friend, a family member, etc.).</item>
/// </list>
/// </remarks>
public sealed class Persona : AuditableEntity
{
    public required PersonaId Id { get; init; }

    public required UserId? UserId { get; init; }

    public required string Name { get; init; }
}