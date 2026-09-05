using Microsoft.Extensions.DependencyInjection;
using OweMe.Domain.Users;

namespace OweMe.Application.User;

public static class DependencyInjection
{
    public static void SetUserContext(this IServiceProvider services, UserId id, string email)
    {
        ArgumentNullException.ThrowIfNull(services);
        if(id == default) throw new ArgumentException("UserId cannot be default.", nameof(id));
        ArgumentException.ThrowIfNullOrEmpty(email);

        var userContextSetter = services.GetRequiredService<IUserContextSetter>();
        userContextSetter.SetContext(id, email);
    }
    
    internal static IServiceCollection AddUserContext(this IServiceCollection services)
    {
        services.AddScoped<UserContextManager>();
        services.AddScoped<IUserContext>(sp => sp.GetRequiredService<UserContextManager>());
        services.AddScoped<IUserContextSetter>(sp => sp.GetRequiredService<UserContextManager>());
        return services;
    }

    private sealed class UserContextManager : IUserContextSetter, IUserContext
    {
        private UserContext? _context;

        public void SetContext(UserId id, string email)
        {
            _context = new UserContext(id, email);
        }

        public UserId Id => _context?.Id ?? throw new UserContextNotAvailableException();
        public string Email => _context?.Email ?? throw new UserContextNotAvailableException();
    }
    
    private interface IUserContextSetter
    {
        void SetContext(UserId id, string email);
    }
    
    private sealed record UserContext(UserId Id, string Email) : IUserContext;
}