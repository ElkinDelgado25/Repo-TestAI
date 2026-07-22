using Api.Application;
using Api.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests.Configuracion.MediatR;

internal static class MediatRTestFactory
{
    public static ServiceProvider Crear(ApiDbContext dbContext)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication();
        services.AddSingleton(dbContext);

        return services.BuildServiceProvider();
    }
}
