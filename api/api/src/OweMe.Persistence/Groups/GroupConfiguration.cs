using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OweMe.Domain.Groups;

namespace OweMe.Persistence.Groups;

internal sealed class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> modelBuilder)
    {
        modelBuilder.HasKey(group => group.Id);

        modelBuilder
            .Property(group => group.Id)
            .HasConversion<GroupIdConverter>();

        modelBuilder
            .Property(group => group.Name)
            .IsRequired()
            .HasMaxLength(GroupConstants.MaxNameLength);
        modelBuilder
            .Property(group => group.Description)
            .HasMaxLength(GroupConstants.MaxDescriptionLength);
    }
}