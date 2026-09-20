using OweMe.Domain.Users;

namespace OweMe.Application;

/// <summary>
/// Represents the current user in the operation context.
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// User's id.
    /// </summary>
    UserId Id { get; }

    /// <summary>
    /// User's email. 
    /// </summary>
    string Email { get; }
}