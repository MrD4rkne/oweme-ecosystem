using Microsoft.Extensions.DependencyInjection;
using OweMe.Domain.Users;

namespace OweMe.Application.User;

public static class DependencyInjection
{
    internal static IServiceCollection AddUserContext(this IServiceCollection services)
    {
        services.AddScoped<UserContextState>();
        services.AddScoped<IUserContext, UserContextAccessor>();
        services.AddScoped<IUserContextSetter, UserContextSetter>();
        return services;
    }

    private sealed class UserContextState
    {
        public UserContext? Value { get; set; }
    }

    private sealed class UserContextAccessor(UserContextState state) : IUserContext
    {
        public UserId Id => state.Value?.Id ?? throw new UserContextNotAvailableException();
        public string Email => state.Value?.Email ?? throw new UserContextNotAvailableException();
    }

    private sealed class UserContextSetter(UserContextState state) : IUserContextSetter
    {
        public void SetContext(UserId id, string email) => state.Value = new UserContext(id, email);
        public void ResetContext() => state.Value = null;
    }

    private sealed record UserContext(UserId Id, string Email);
}