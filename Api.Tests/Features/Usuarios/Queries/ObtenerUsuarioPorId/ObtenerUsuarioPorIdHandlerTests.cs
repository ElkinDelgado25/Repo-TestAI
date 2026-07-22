using Api.Application.Features.Usuarios.Queries.ObtenerUsuarioPorId;
using Api.Tests.Configuracion.Factories;
using Api.Tests.Configuracion.MediatR;
using Api.Tests.Configuracion.Persistencia;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Api.Tests.Features.Usuarios.Queries.ObtenerUsuarioPorId;

public sealed class ObtenerUsuarioPorIdHandlerTests
{
    [Fact]
    public async Task ObtenerUsuarioPorId_DebeRetornarUsuarioCuandoExiste()
    {
        await using var dbContext = ApiDbContextFactory.Crear();
        var id = Guid.NewGuid();
        var usuario = new UsuarioFactory().Crear(
            id: id,
            nombre: "Ana",
            apellido: "Alfaro",
            email: "ana@example.test");
        dbContext.Usuarios.Add(usuario);
        await dbContext.SaveChangesAsync();

        var resultado = await new ObtenerUsuarioPorIdHandler(dbContext)
            .Handle(new ObtenerUsuarioPorIdQuery(id), CancellationToken.None);

        resultado.ShouldNotBeNull();
        resultado.Id.ShouldBe(id);
        resultado.Nombre.ShouldBe("Ana");
        resultado.Apellido.ShouldBe("Alfaro");
        resultado.Email.ShouldBe("ana@example.test");
    }

    [Fact]
    public async Task ObtenerUsuarioPorId_CuandoNoExiste_DebeRetornarNulo()
    {
        await using var dbContext = ApiDbContextFactory.Crear();

        var resultado = await new ObtenerUsuarioPorIdHandler(dbContext)
            .Handle(new ObtenerUsuarioPorIdQuery(Guid.NewGuid()), CancellationToken.None);

        resultado.ShouldBeNull();
    }

    [Fact]
    public async Task ObtenerUsuarioPorId_ConIdVacio_DebeLanzarExcepcionDeValidacion()
    {
        await using var dbContext = ApiDbContextFactory.Crear();
        using var services = MediatRTestFactory.Crear(dbContext);
        var sender = services.GetRequiredService<ISender>();

        await Should.ThrowAsync<ValidationException>(
            () => sender.Send(new ObtenerUsuarioPorIdQuery(Guid.Empty), CancellationToken.None));
    }
}
