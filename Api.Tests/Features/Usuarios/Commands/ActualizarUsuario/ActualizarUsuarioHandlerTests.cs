using Api.Application.Exceptions;
using Api.Application.Features.Usuarios.Commands.ActualizarUsuario;
using Api.Tests.Configuracion.Factories;
using Api.Tests.Configuracion.MediatR;
using Api.Tests.Configuracion.Persistencia;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Api.Tests.Features.Usuarios.Commands.ActualizarUsuario;

public sealed class ActualizarUsuarioHandlerTests
{
    [Fact]
    public async Task ActualizarUsuario_DebeGuardarLosCambios()
    {
        await using var dbContext = ApiDbContextFactory.Crear();
        var id = Guid.NewGuid();
        dbContext.Usuarios.Add(new UsuarioFactory().Crear(id: id));
        await dbContext.SaveChangesAsync();
        var handler = new ActualizarUsuarioHandler(dbContext);

        var resultado = await handler.Handle(
            new ActualizarUsuarioCommand(
                id,
                "Ana",
                "Alfaro",
                "ana.actualizada@example.test"),
            CancellationToken.None);

        resultado.ShouldNotBeNull();
        resultado.Nombre.ShouldBe("Ana");
        resultado.Apellido.ShouldBe("Alfaro");
        resultado.Email.ShouldBe("ana.actualizada@example.test");

        var usuarioActualizado = await dbContext.Usuarios.SingleAsync();
        usuarioActualizado.Nombre.Value.ShouldBe("Ana");
        usuarioActualizado.Email.Value.ShouldBe("ana.actualizada@example.test");
    }

    [Fact]
    public async Task ActualizarUsuario_CuandoNoExiste_DebeRetornarNulo()
    {
        await using var dbContext = ApiDbContextFactory.Crear();
        var handler = new ActualizarUsuarioHandler(dbContext);

        var resultado = await handler.Handle(
            new ActualizarUsuarioCommand(
                Guid.NewGuid(),
                "Ana",
                "Alfaro",
                "ana@example.test"),
            CancellationToken.None);

        resultado.ShouldBeNull();
    }

    [Fact]
    public async Task ActualizarUsuario_ConEmailDeOtroUsuario_DebeLanzarExcepcion()
    {
        await using var dbContext = ApiDbContextFactory.Crear();
        var usuarios = new UsuarioFactory();
        var usuarioAActualizar = usuarios.Crear(email: "ana@example.test");
        var otroUsuario = usuarios.Crear(email: "otro@example.test");
        dbContext.Usuarios.AddRange(usuarioAActualizar, otroUsuario);
        await dbContext.SaveChangesAsync();
        var handler = new ActualizarUsuarioHandler(dbContext);

        await Should.ThrowAsync<UsuarioEmailDuplicadoException>(
            () => handler.Handle(
                new ActualizarUsuarioCommand(
                    usuarioAActualizar.Id.Value,
                    usuarioAActualizar.Nombre.Value,
                    usuarioAActualizar.Apellido.Value,
                    otroUsuario.Email.Value),
                CancellationToken.None));
    }

    [Fact]
    public async Task ActualizarUsuario_ConNombreVacio_DebeLanzarExcepcionDeValidacion()
    {
        await using var dbContext = ApiDbContextFactory.Crear();
        using var services = MediatRTestFactory.Crear(dbContext);
        var sender = services.GetRequiredService<ISender>();

        await Should.ThrowAsync<ValidationException>(
            () => sender.Send(
                new ActualizarUsuarioCommand(
                    Guid.NewGuid(),
                    string.Empty,
                    "Alfaro",
                    "ana@example.test"),
                CancellationToken.None));
    }
}
