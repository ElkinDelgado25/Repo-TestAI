using Api.Application.Features.Usuarios.Queries.ObtenerUsuarios;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;

namespace Api.Tests.Features.Usuarios.Queries.ObtenerUsuarios;

public sealed class ObtenerUsuariosControllerTests
{
    [Fact]
    public async Task ObtenerUsuarios_Controller_DebeRetornarHttpOk()
    {
        var respuestaEsperada = new UsuariosPaginadosDto(
            [],
            Pagina: 1,
            TamanoPagina: 10,
            TotalRegistros: 0,
            TotalPaginas: 0);
        var sender = new Mock<ISender>();
        sender
            .Setup(mediator => mediator.Send(
                It.IsAny<ObtenerUsuariosQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(respuestaEsperada);
        var controller = new ObtenerUsuariosController(sender.Object);

        var resultado = await controller.ObtenerAsync(
            new ObtenerUsuariosQuery(),
            CancellationToken.None);

        var ok = resultado.Result.ShouldBeOfType<OkObjectResult>();
        ok.StatusCode.ShouldBe(StatusCodes.Status200OK);
        ok.Value.ShouldBe(respuestaEsperada);
    }

    [Fact]
    public async Task ObtenerUsuarios_Controller_ConPaginacionInvalida_DebeRetornarBadRequest()
    {
        var sender = new Mock<ISender>();
        sender
            .Setup(mediator => mediator.Send(
                It.IsAny<ObtenerUsuariosQuery>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(
                [new ValidationFailure(nameof(ObtenerUsuariosQuery.Pagina), "La página debe ser mayor o igual a uno.")]));
        var controller = new ObtenerUsuariosController(sender.Object);

        var resultado = await controller.ObtenerAsync(
            new ObtenerUsuariosQuery(Pagina: 0),
            CancellationToken.None);

        var badRequest = resultado.Result.ShouldBeOfType<BadRequestObjectResult>();
        badRequest.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        var details = badRequest.Value.ShouldBeOfType<ValidationProblemDetails>();
        details.Errors.ContainsKey(nameof(ObtenerUsuariosQuery.Pagina)).ShouldBeTrue();
    }
}
