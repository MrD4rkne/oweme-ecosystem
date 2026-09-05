using OweMe.Application;
using OweMe.Application.User;

namespace OweMe.Api.User;

public static class UserRegistration
{
    public static ApplicationBuilder UseUserContext(this ApplicationBuilder app)
    {
        ThrowIfUserContextNotRegistered(app.ApplicationServices);
        app.UseMiddleware<UserContextMiddleware>();
        return app;
    }

    private static void ThrowIfUserContextNotRegistered(IServiceProvider serviceProvider)
    {
        var userContextProvider = serviceProvider.GetService<IUserContext>();
        if (userContextProvider == null)
        {
            throw new InvalidOperationException($"User context services are not registered.");
        }
    }
}
