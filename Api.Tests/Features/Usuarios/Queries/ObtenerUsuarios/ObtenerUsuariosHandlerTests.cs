using Api.Application.Features.Usuarios.Queries.ObtenerUsuarios;
using Api.Tests.Configuracion.Factories;
using Api.Tests.Configuracion.MediatR;
using Api.Tests.Configuracion.Persistencia;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Api.Tests.Features.Usuarios.Queries.ObtenerUsuarios;

public sealed class ObtenerUsuariosHandlerTests
{
    [Fact]
    public async Task ObtenerUsuarios_DebeRetornarListaPaginada()
    {
        await using var dbContext = ApiDbContextFactory.Crear();
        var usuarios = new UsuarioFactory();

        dbContext.Usuarios.Add(usuarios.Crear(nombre: "Zoe", apellido: "Alfaro"));
        dbContext.Usuarios.Add(usuarios.Crear(nombre: "Ana", apellido: "Bravo"));
        dbContext.Usuarios.Add(usuarios.Crear(nombre: "Mario", apellido: "Castro"));
        await dbContext.SaveChangesAsync();

        var resultado = await new ObtenerUsuariosHandler(dbContext)
            .Handle(new ObtenerUsuariosQuery(Pagina: 1, TamanoPagina: 2), CancellationToken.None);

        resultado.Pagina.ShouldBe(1);
        resultado.TamanoPagina.ShouldBe(2);
        resultado.TotalRegistros.ShouldBe(3);
        resultado.TotalPaginas.ShouldBe(2);
        resultado.Usuarios.Count.ShouldBe(2);
        resultado.Usuarios[0].Apellido.ShouldBe("Alfaro");
        resultado.Usuarios[1].Apellido.ShouldBe("Bravo");
    }

    [Fact]
    public async Task ObtenerUsuarios_DebeLanzarExcepcionSiPaginacionEsInvalida()
    {
        await using var dbContext = ApiDbContextFactory.Crear();
        using var services = MediatRTestFactory.Crear(dbContext);
        var sender = services.GetRequiredService<ISender>();

        await Should.ThrowAsync<ValidationException>(
            () => sender.Send(new ObtenerUsuariosQuery(Pagina: 0), CancellationToken.None));
    }

    [Fact]
    public async Task ObtenerUsuarios_DebeLanzarExcepcionSiTamanoPaginaExcedeElMaximo()
    {
        await using var dbContext = ApiDbContextFactory.Crear();
        using var services = MediatRTestFactory.Crear(dbContext);
        var sender = services.GetRequiredService<ISender>();

        await Should.ThrowAsync<ValidationException>(
            () => sender.Send(new ObtenerUsuariosQuery(TamanoPagina: 101), CancellationToken.None));
    }

    [Fact]
    public async Task ObtenerUsuarios_SinRegistros_DebeRetornarPaginaVacia()
    {
        await using var dbContext = ApiDbContextFactory.Crear();

        var resultado = await new ObtenerUsuariosHandler(dbContext)
            .Handle(new ObtenerUsuariosQuery(), CancellationToken.None);

        resultado.Usuarios.Count.ShouldBe(0);
        resultado.TotalRegistros.ShouldBe(0);
        resultado.TotalPaginas.ShouldBe(0);
    }

    [Fact]
    public async Task ObtenerUsuarios_ConPaginaFueraDeRango_DebeRetornarListaVacia()
    {
        await using var dbContext = ApiDbContextFactory.Crear();
        var usuarios = new UsuarioFactory();

        dbContext.Usuarios.Add(usuarios.Crear());
        dbContext.Usuarios.Add(usuarios.Crear());
        dbContext.Usuarios.Add(usuarios.Crear());
        await dbContext.SaveChangesAsync();

        var resultado = await new ObtenerUsuariosHandler(dbContext)
            .Handle(new ObtenerUsuariosQuery(Pagina: 3, TamanoPagina: 2), CancellationToken.None);

        resultado.Usuarios.Count.ShouldBe(0);
        resultado.Pagina.ShouldBe(3);
        resultado.TotalRegistros.ShouldBe(3);
        resultado.TotalPaginas.ShouldBe(2);
    }
}
