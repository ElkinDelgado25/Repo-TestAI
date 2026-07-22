using Api.Application.Exceptions;
using Api.Application.Features.Usuarios.Commands.RegistrarUsuario;
using Api.Tests.Configuracion.Factories;
using Api.Tests.Configuracion.MediatR;
using Api.Tests.Configuracion.Persistencia;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Vogen;

namespace Api.Tests.Features.Usuarios.Commands.RegistrarUsuario;

public sealed class RegistrarUsuarioHandlerTests
{
    [Fact]
    public async Task RegistrarUsuario_DebeGuardarUsuarioEnBaseDeDatos()
    {
        await using var dbContext = ApiDbContextFactory.Crear();
        var comando = new UsuarioFactory().CrearComando();
        var handler = new RegistrarUsuarioHandler(dbContext);

        var resultado = await handler.Handle(comando, CancellationToken.None);

        resultado.Id.ShouldNotBe(Guid.Empty);
        resultado.Nombre.ShouldBe(comando.Nombre);
        resultado.Apellido.ShouldBe(comando.Apellido);
        resultado.Email.ShouldBe(comando.Email);

        var usuarioGuardado = await dbContext.Usuarios.SingleAsync();
        usuarioGuardado.Id.Value.ShouldBe(resultado.Id);
        usuarioGuardado.Email.Value.ShouldBe(comando.Email);
    }

    [Fact]
    public async Task RegistrarUsuario_DebeLanzarExcepcionSiEmailEsInvalido()
    {
        await using var dbContext = ApiDbContextFactory.Crear();
        var comando = new UsuarioFactory().CrearComando() with { Email = "correo-invalido" };
        var handler = new RegistrarUsuarioHandler(dbContext);

        await Should.ThrowAsync<ValueObjectValidationException>(
            () => handler.Handle(comando, CancellationToken.None));

        (await dbContext.Usuarios.CountAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task RegistrarUsuario_DebeNormalizarElEmailAntesDePersistirlo()
    {
        await using var dbContext = ApiDbContextFactory.Crear();
        var comando = new UsuarioFactory().CrearComando() with
        {
            Email = "  USUARIO@EXAMPLE.TEST ",
        };
        var handler = new RegistrarUsuarioHandler(dbContext);

        var resultado = await handler.Handle(comando, CancellationToken.None);

        resultado.Email.ShouldBe("usuario@example.test");
        (await dbContext.Usuarios.SingleAsync()).Email.Value.ShouldBe("usuario@example.test");
    }

    [Fact]
    public async Task RegistrarUsuario_DebeLanzarExcepcionSiElNombreEsVacio()
    {
        await using var dbContext = ApiDbContextFactory.Crear();
        using var services = MediatRTestFactory.Crear(dbContext);
        var sender = services.GetRequiredService<ISender>();
        var comando = new UsuarioFactory().CrearComando() with { Nombre = string.Empty };

        await Should.ThrowAsync<ValidationException>(
            () => sender.Send(comando, CancellationToken.None));

        (await dbContext.Usuarios.CountAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task RegistrarUsuario_DebeLanzarExcepcionSiElCorreoYaExiste()
    {
        await using var dbContext = ApiDbContextFactory.Crear();
        var comando = new UsuarioFactory().CrearComando(email: "repetido@example.test");
        var handler = new RegistrarUsuarioHandler(dbContext);

        await handler.Handle(comando, CancellationToken.None);

        var exception = await Should.ThrowAsync<UsuarioEmailDuplicadoException>(
            () => handler.Handle(comando, CancellationToken.None));

        exception.Message.ShouldContain(comando.Email);
    }
}
