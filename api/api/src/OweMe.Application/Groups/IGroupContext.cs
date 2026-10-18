using Microsoft.EntityFrameworkCore;
using OweMe.Domain.Groups;

namespace OweMe.Application.Groups;

public interface IGroupContext
{
    DbSet<Group> Groups { get; }

    DbSet<GroupMember> GroupMembers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}