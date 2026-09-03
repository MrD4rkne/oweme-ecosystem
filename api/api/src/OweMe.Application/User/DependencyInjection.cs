using Microsoft.Extensions.DependencyInjection;

namespace OweMe.Application.User;

public static class DependencyInjection
{
    public static void UseUserContext(this IServiceProvider services, IUserContext context)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(context);

        var userContextSetter = services.GetRequiredService<IUserContextSetter>();
        userContextSetter.SetContext(context);
    }
    
    internal static IServiceCollection AddUserContext(this IServiceCollection services)
    {
        services.AddScoped<UserContextProvider>();
        services.AddScoped<IUserContextProvider>(sp => sp.GetRequiredService<UserContextProvider>());
        services.AddScoped<IUserContextSetter>(sp => sp.GetRequiredService<UserContextProvider>());
        return services;
    }

    private sealed class UserContextProvider : IUserContextProvider, IUserContextSetter
    {
        private IUserContext? _context;

        public IUserContext Context =>
            _context ?? throw new InvalidOperationException("User context has not been set.");

        public void SetContext(IUserContext context)
        {
            _context = context;
        }
    }
    
    private interface IUserContextSetter
    {
        void SetContext(IUserContext context);
    }
}