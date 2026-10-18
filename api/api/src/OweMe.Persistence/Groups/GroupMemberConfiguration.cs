using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OweMe.Domain.Groups;
using OweMe.Persistence.Personas;

namespace OweMe.Persistence.Groups;

internal sealed class GroupMemberConfiguration : IEntityTypeConfiguration<GroupMember>
{
    public void Configure(EntityTypeBuilder<GroupMember> modelBuilder)
    {
        modelBuilder.HasKey(membership => new { membership.GroupId, membership.PersonaId });

        modelBuilder
            .Property(group => group.GroupId)
            .HasConversion<GroupIdConverter>();

        modelBuilder
            .Property(group => group.PersonaId)
            .HasConversion<PersonaIdConverter>();
    }
}