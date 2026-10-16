using Microsoft.EntityFrameworkCore;
using OweMe.Domain.Users;

namespace OweMe.Application.Personas;

public interface IPersonaContext
{
    DbSet<Persona> Personas { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}