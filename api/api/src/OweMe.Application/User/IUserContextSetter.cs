using OweMe.Domain.Users;

namespace OweMe.Application.User;

public interface IUserContextSetter
{
    void SetContext(UserId id, string email);

    void ResetContext();
}