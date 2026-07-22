using Api.Application.Features.Usuarios.Queries.ObtenerUsuarioPorId;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;

namespace Api.Tests.Features.Usuarios.Queries.ObtenerUsuarioPorId;

public sealed class ObtenerUsuarioPorIdControllerTests
{
    [Fact]
    public async Task ObtenerUsuarioPorId_Controller_CuandoExiste_DebeRetornarHttpOk()
    {
        var usuario = new UsuarioPorIdDto(
            Guid.NewGuid(),
            "Ana",
            "Alfaro",
            "ana@example.test");
        var sender = new Mock<ISender>();
        sender
            .Setup(mediator => mediator.Send(
                It.IsAny<ObtenerUsuarioPorIdQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        var controller = new ObtenerUsuarioPorIdController(sender.Object);

        var resultado = await controller.ObtenerAsync(usuario.Id, CancellationToken.None);

        var ok = resultado.Result.ShouldBeOfType<OkObjectResult>();
        ok.StatusCode.ShouldBe(StatusCodes.Status200OK);
        ok.Value.ShouldBe(usuario);
    }

    [Fact]
    public async Task ObtenerUsuarioPorId_Controller_CuandoNoExiste_DebeRetornarNotFound()
    {
        var sender = new Mock<ISender>();
        sender
            .Setup(mediator => mediator.Send(
                It.IsAny<ObtenerUsuarioPorIdQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((UsuarioPorIdDto?)null);
        var controller = new ObtenerUsuarioPorIdController(sender.Object);

        var resultado = await controller.ObtenerAsync(Guid.NewGuid(), CancellationToken.None);

        resultado.Result.ShouldBeOfType<NotFoundResult>();
    }
}
