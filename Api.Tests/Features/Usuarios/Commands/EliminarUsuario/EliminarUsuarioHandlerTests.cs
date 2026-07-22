using Api.Application.Features.Usuarios.Commands.EliminarUsuario;
using Api.Tests.Configuracion.Factories;
using Api.Tests.Configuracion.MediatR;
using Api.Tests.Configuracion.Persistencia;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Api.Tests.Features.Usuarios.Commands.EliminarUsuario;

public sealed class EliminarUsuarioHandlerTests
{
    [Fact]
    public async Task EliminarUsuario_DebeMarcarRegistroComoEliminadoSinBorrarlo()
    {
        await using var dbContext = ApiDbContextFactory.Crear();
        var usuario = new UsuarioFactory().Crear();
        dbContext.Usuarios.Add(usuario);
        await dbContext.SaveChangesAsync();
        var handler = new EliminarUsuarioHandler(dbContext);

        var eliminado = await handler.Handle(
            new EliminarUsuarioCommand(usuario.Id.Value),
            CancellationToken.None);

        eliminado.ShouldBeTrue();
        (await dbContext.Usuarios.CountAsync()).ShouldBe(0);

        var usuarioEliminado = await dbContext.Usuarios
            .IgnoreQueryFilters()
            .SingleAsync();
        usuarioEliminado.EstaEliminado.ShouldBeTrue();
        usuarioEliminado.EliminadoEn.ShouldNotBeNull();
    }

    [Fact]
    public async Task EliminarUsuario_CuandoNoExiste_DebeRetornarFalso()
    {
        await using var dbContext = ApiDbContextFactory.Crear();
        var handler = new EliminarUsuarioHandler(dbContext);

        var eliminado = await handler.Handle(
            new EliminarUsuarioCommand(Guid.NewGuid()),
            CancellationToken.None);

        eliminado.ShouldBeFalse();
    }

    [Fact]
    public async Task EliminarUsuario_ConIdVacio_DebeLanzarExcepcionDeValidacion()
    {
        await using var dbContext = ApiDbContextFactory.Crear();
        using var services = MediatRTestFactory.Crear(dbContext);
        var sender = services.GetRequiredService<ISender>();

        await Should.ThrowAsync<ValidationException>(
            () => sender.Send(new EliminarUsuarioCommand(Guid.Empty), CancellationToken.None));
    }
}
