using Api.Application.Features.Usuarios.Commands.RegistrarUsuario;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Bogus;

namespace Api.Tests.Configuracion.Factories;

internal sealed class UsuarioFactory
{
    private readonly Faker _faker = new("es");

    public Usuario Crear(
        Guid? id = null,
        string? nombre = null,
        string? apellido = null,
        string? email = null)
    {
        return new Usuario(
            UsuarioId.From(id ?? _faker.Random.Guid()),
            Nombre.From(nombre ?? _faker.Name.FirstName()),
            Apellido.From(apellido ?? _faker.Name.LastName()),
            Email.From(email ?? $"usuario-{_faker.Random.Guid():N}@example.test"));
    }

    public RegistrarUsuarioCommand CrearComando(
        string? nombre = null,
        string? apellido = null,
        string? email = null)
    {
        var usuario = Crear(nombre: nombre, apellido: apellido, email: email);

        return new RegistrarUsuarioCommand(
            usuario.Nombre.Value,
            usuario.Apellido.Value,
            usuario.Email.Value);
    }
}
