using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OweMe.Domain.Users;

namespace OweMe.Persistence.User;

public sealed class UserIdConverter()
    : ValueConverter<UserId, Guid>(id => id.Value, v => new UserId(v));