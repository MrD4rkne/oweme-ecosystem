using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OweMe.Domain.Groups;
using OweMe.Domain.Users;
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

        modelBuilder
            .HasOne<Group>()
            .WithMany()
            .HasForeignKey(membership => membership.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: a persona referenced by a group must not silently vanish from it.
        modelBuilder
            .HasOne<Persona>()
            .WithMany()
            .HasForeignKey(membership => membership.PersonaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}