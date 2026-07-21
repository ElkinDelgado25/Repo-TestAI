using Api.Application.Abstractions.Persistence;
using Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("bd")
            ?? throw new InvalidOperationException("No se configuró la cadena de conexión 'bd'.");

        services.AddDbContext<ApiDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IApplicationDbContext>(serviceProvider =>
            serviceProvider.GetRequiredService<ApiDbContext>());

        return services;
    }
}
