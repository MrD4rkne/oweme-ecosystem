using Microsoft.Extensions.DependencyInjection;
using OweMe.Domain.Users;

namespace OweMe.Application.User;

public static class ServiceProviderExtensions
{
    public static void SetUserContext(this IServiceProvider? services, UserId id, string email)
    {
        ArgumentNullException.ThrowIfNull(services);
        if(id == default) throw new ArgumentException("UserId cannot be default.", nameof(id));
        ArgumentException.ThrowIfNullOrEmpty(email);

        var userContextSetter = services.GetRequiredService<IUserContextSetter>();
        userContextSetter.SetContext(id, email);
    }
    
    public static void ResetUserContext(this IServiceProvider? services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var userContextSetter = services.GetRequiredService<IUserContextSetter>();
        userContextSetter.ResetContext();
    }
}