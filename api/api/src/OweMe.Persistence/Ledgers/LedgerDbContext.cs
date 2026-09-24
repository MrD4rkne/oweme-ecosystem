using Microsoft.EntityFrameworkCore;
using OweMe.Application;
using OweMe.Application.Groups;
using OweMe.Domain.Groups;
using OweMe.Persistence.Common;

namespace OweMe.Persistence.Groups;

public class GroupDbContext : AuditableDbContext, IGroupContext
{
    public GroupDbContext(DbContextOptions<GroupDbContext> options, TimeProvider timeProvider,
        IUserContext userContext) : base(options, timeProvider, userContext)
    {
    }

    public DbSet<Group> Groups { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Group>()
            .HasKey(l => l.Id);

        modelBuilder.Entity<Group>()
            .Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(GroupConstants.MaxNameLength);
        modelBuilder.Entity<Group>()
            .Property(l => l.Description)
            .HasMaxLength(GroupConstants.MaxDescriptionLength);

        base.OnModelCreating(modelBuilder);
    }
}