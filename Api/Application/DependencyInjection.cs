using Api.Application.Behaviors;
using Api.Application.Features.Usuarios.Commands.RegistrarUsuario;
using FluentValidation;
using MediatR;

namespace Api.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssemblyContaining<RegistrarUsuarioHandler>();
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        services.AddValidatorsFromAssemblyContaining<RegistrarUsuarioHandler>();

        return services;
    }
}
