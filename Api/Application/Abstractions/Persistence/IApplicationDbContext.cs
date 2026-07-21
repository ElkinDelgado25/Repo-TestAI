using Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Abstractions.Persistence;

public interface IApplicationDbContext
{
    DbSet<Usuario> Usuarios { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
