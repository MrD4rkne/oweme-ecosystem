using System.Security.Claims;
using OweMe.Application;
using OweMe.Application.User;
using OweMe.Domain.Users;

namespace OweMe.Api.User;

public class UserContextMiddleware(
    ILogger<UserContextMiddleware> logger) : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = context.User.FindFirstValue(ClaimTypes.Email);

            if(string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(email))
            {
                logger.LogWarning("User context is missing required information.");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }

            context.RequestServices.SetUserContext(new UserId(Guid.Parse(userId)), email);
        }

        return next(context);
    }
}
