namespace Api.Infrastructure.Api.Contracts;

public sealed record UsuarioResponse(Guid Id, string Nombre, string Apellido, string Email);
