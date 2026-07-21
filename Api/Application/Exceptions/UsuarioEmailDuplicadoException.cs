namespace Api.Application.Exceptions;

public sealed class UsuarioEmailDuplicadoException(string email)
    : Exception($"Ya existe un usuario con el correo '{email}'.");
