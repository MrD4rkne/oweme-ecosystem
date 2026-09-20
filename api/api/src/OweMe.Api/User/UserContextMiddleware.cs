using System.Security.Claims;
using OweMe.Application.User;
using OweMe.Domain.Users;

namespace OweMe.Api.User;
public static class UserContextWolverineMiddleware
{
    public static void Before(
        IUserContextSetter userContext,
        IHttpContextAccessor httpContextAccessor)
    {
        var principal = httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var userIdStr = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = principal.FindFirstValue(ClaimTypes.Email);

        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(userIdStr) && Guid.TryParse(userIdStr, out var guid))
        {
            userContext.SetContext(new UserId(guid), email);
        }
    }
}
