using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Bogus;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Infrastructure.Data.Seed;

public static class DbInitializer
{
    public static async Task InitializeDatabaseAsync(
        this WebApplication app,
        CancellationToken cancellationToken = default
    )
    {
        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApiDbContext>();

        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        await dbContext.EnsureSoftDeleteColumnsAsync(cancellationToken);

        if (await dbContext.Usuarios.IgnoreQueryFilters().AnyAsync(cancellationToken))
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
                    Email.From($"usuario{indice:D3}@seed.example")
                );
            })
            .Generate(25);

        await dbContext.Usuarios.AddRangeAsync(usuarios, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureSoftDeleteColumnsAsync(
        this ApiDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (!dbContext.Database.IsSqlServer())
        {
            return;
        }

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            IF COL_LENGTH('dbo.Usuarios', 'EstaEliminado') IS NULL
            BEGIN
                ALTER TABLE [dbo].[Usuarios]
                ADD [EstaEliminado] bit NOT NULL
                    CONSTRAINT [DF_Usuarios_EstaEliminado] DEFAULT CONVERT(bit, 0) WITH VALUES;
            END

            IF COL_LENGTH('dbo.Usuarios', 'EliminadoEn') IS NULL
            BEGIN
                ALTER TABLE [dbo].[Usuarios]
                ADD [EliminadoEn] datetimeoffset NULL;
            END
            """,
            cancellationToken);
    }
}
