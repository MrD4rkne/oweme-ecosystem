using Microsoft.AspNetCore.Identity;

namespace OweMe.Identity.Persistence.Users.Domain;

#pragma warning disable S2094 // Remove this empty class, write its code or make it an "interface".
/// <summary>
/// Represents an application user in the system.
/// </summary>
public sealed class ApplicationUser : IdentityUser;
#pragma warning restore S2094 // Remove this empty class, write its code or make it an "interface".