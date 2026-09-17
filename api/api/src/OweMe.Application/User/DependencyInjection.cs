using Microsoft.Extensions.DependencyInjection;
using OweMe.Domain.Users;

namespace OweMe.Application.User;

public static class DependencyInjection
{
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
        
        public void ResetContext()
        {
            _context = null;
        }

        public UserId Id => _context?.Id ?? throw new UserContextNotAvailableException();
        public string Email => _context?.Email ?? throw new UserContextNotAvailableException();
    }
    
    private sealed record UserContext(UserId Id, string Email);
}