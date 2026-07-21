namespace Api.Application.Features.Usuarios.Queries.ObtenerUsuario;

public sealed record ObtenerUsuarioResult(Guid Id, string Nombre, string Apellido, string Email);
