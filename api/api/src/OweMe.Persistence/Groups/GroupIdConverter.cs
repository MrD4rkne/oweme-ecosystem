using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OweMe.Domain.Groups;

namespace OweMe.Persistence.Groups;

public sealed class GroupIdConverter()
    : ValueConverter<GroupId, Guid>(id => id.Value, v => new GroupId(v));