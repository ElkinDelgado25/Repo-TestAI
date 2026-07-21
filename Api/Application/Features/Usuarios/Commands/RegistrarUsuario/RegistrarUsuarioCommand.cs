using MediatR;

namespace Api.Application.Features.Usuarios.Commands.RegistrarUsuario;

public sealed record RegistrarUsuarioCommand(string Nombre, string Apellido, string Email)
    : IRequest<RegistrarUsuarioResult>;
