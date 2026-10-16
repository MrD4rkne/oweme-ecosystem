using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OweMe.Application.Personas;
using OweMe.Domain.Users;
using OweMe.Persistence.User;

namespace OweMe.Persistence.Personas;

internal sealed class PersonaConfiguration : IEntityTypeConfiguration<Persona>
{
    public void Configure(EntityTypeBuilder<Persona> modelBuilder)
    {
        modelBuilder
            .HasKey(l => l.Id);

        modelBuilder
            .Property(p => p.Id)
            .HasConversion<PersonaIdConverter>();

        modelBuilder
            .Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(PersonaConstants.MaxNameLength);

        modelBuilder
            .Property(p => p.UserId)
            .HasConversion<UserIdConverter>();
    }
}