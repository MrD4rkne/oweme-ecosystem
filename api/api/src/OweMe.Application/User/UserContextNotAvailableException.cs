namespace OweMe.Application.User;

public sealed class UserContextNotAvailableException() : Exception("User context is not available. Set the user context in the current scope.");