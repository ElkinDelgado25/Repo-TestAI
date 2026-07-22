using Api.Application.Features.Usuarios.Commands.EliminarUsuario;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;

namespace Api.Tests.Features.Usuarios.Commands.EliminarUsuario;

public sealed class EliminarUsuarioControllerTests
{
    [Fact]
    public async Task EliminarUsuario_Controller_CuandoExiste_DebeRetornarNoContent()
    {
        var sender = new Mock<ISender>();
        sender
            .Setup(mediator => mediator.Send(
                It.IsAny<EliminarUsuarioCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var controller = new EliminarUsuarioController(sender.Object);

        var resultado = await controller.EliminarAsync(Guid.NewGuid(), CancellationToken.None);

        var noContent = resultado.ShouldBeOfType<NoContentResult>();
        noContent.StatusCode.ShouldBe(StatusCodes.Status204NoContent);
    }

    [Fact]
    public async Task EliminarUsuario_Controller_CuandoNoExiste_DebeRetornarNotFound()
    {
        var sender = new Mock<ISender>();
        sender
            .Setup(mediator => mediator.Send(
                It.IsAny<EliminarUsuarioCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var controller = new EliminarUsuarioController(sender.Object);

        var resultado = await controller.EliminarAsync(Guid.NewGuid(), CancellationToken.None);

        resultado.ShouldBeOfType<NotFoundResult>();
    }
}
