using Microsoft.EntityFrameworkCore;
using OweMe.Application;
using OweMe.Application.Personas;
using OweMe.Domain.Users;
using OweMe.Persistence.Common;
using OweMe.Persistence.User;

namespace OweMe.Persistence.Personas;

public sealed class PersonaDbContext(
    DbContextOptions<PersonaDbContext> options,
    TimeProvider timeProvider,
    IUserContext userContext)
    : AuditableDbContext(options, timeProvider, userContext), IPersonaContext
{
    public DbSet<Persona> Personas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Persona>()
            .HasKey(l => l.Id);

        modelBuilder.Entity<Persona>()
            .Property(p => p.Id)
            .HasConversion<PersonaIdConverter>();

        modelBuilder.Entity<Persona>()
            .Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(PersonaConstants.MaxNameLength);

        modelBuilder.Entity<Persona>()
            .Property(p => p.UserId)
            .HasConversion<UserIdConverter>();

        base.OnModelCreating(modelBuilder);
    }
}