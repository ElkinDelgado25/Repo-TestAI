using Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Tests.Configuracion.Persistencia;

internal static class ApiDbContextFactory
{
    public static ApiDbContext Crear()
    {
        var options = new DbContextOptionsBuilder<ApiDbContext>()
            .UseInMemoryDatabase($"api-tests-{Guid.NewGuid():N}")
            .Options;

        return new ApiDbContext(options);
    }
}
