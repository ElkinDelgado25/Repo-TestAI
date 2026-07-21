namespace Api.Application.Features.Usuarios.Commands.RegistrarUsuario;

public sealed record RegistrarUsuarioResult(Guid Id, string Nombre, string Apellido, string Email);
