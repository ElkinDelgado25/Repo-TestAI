using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Infrastructure.Data.Seed;

public static class DatabaseInitializer
{
    public static async Task InicializarAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApiDbContext>();

        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (await dbContext.Usuarios.AnyAsync(cancellationToken))
        {
            return;
        }

        Randomizer.Seed = new Random(20260720);
        var indice = 0;
        var usuarios = new Faker<Usuario>("es")
            .CustomInstantiator(faker =>
            {
                indice++;

                return new Usuario(
                    UsuarioId.From(faker.Random.Guid()),
                    Nombre.From(faker.Name.FirstName()),
                    Apellido.From(faker.Name.LastName()),
                    Email.From($"usuario{indice:D3}@seed.example"));
            })
            .Generate(25);

        await dbContext.Usuarios.AddRangeAsync(usuarios, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
