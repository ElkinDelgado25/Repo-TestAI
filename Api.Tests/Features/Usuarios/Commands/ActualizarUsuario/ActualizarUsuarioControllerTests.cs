using Api.Application.Features.Usuarios.Commands.ActualizarUsuario;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shouldly;

namespace Api.Tests.Features.Usuarios.Commands.ActualizarUsuario;

public sealed class ActualizarUsuarioControllerTests
{
    [Fact]
    public async Task ActualizarUsuario_Controller_CuandoExiste_DebeRetornarHttpOk()
    {
        var usuario = new ActualizarUsuarioDto(
            Guid.NewGuid(),
            "Ana",
            "Alfaro",
            "ana@example.test");
        var sender = new Mock<ISender>();
        sender
            .Setup(mediator => mediator.Send(
                It.IsAny<ActualizarUsuarioCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        var controller = new ActualizarUsuarioController(sender.Object);

        var resultado = await controller.ActualizarAsync(
            usuario.Id,
            new ActualizarUsuarioRequest(usuario.Nombre, usuario.Apellido, usuario.Email),
            CancellationToken.None);

        var ok = resultado.Result.ShouldBeOfType<OkObjectResult>();
        ok.StatusCode.ShouldBe(StatusCodes.Status200OK);
        ok.Value.ShouldBe(usuario);
    }

    [Fact]
    public async Task ActualizarUsuario_Controller_CuandoNoExiste_DebeRetornarNotFound()
    {
        var sender = new Mock<ISender>();
        sender
            .Setup(mediator => mediator.Send(
                It.IsAny<ActualizarUsuarioCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActualizarUsuarioDto?)null);
        var controller = new ActualizarUsuarioController(sender.Object);

        var resultado = await controller.ActualizarAsync(
            Guid.NewGuid(),
            new ActualizarUsuarioRequest("Ana", "Alfaro", "ana@example.test"),
            CancellationToken.None);

        resultado.Result.ShouldBeOfType<NotFoundResult>();
    }
}
