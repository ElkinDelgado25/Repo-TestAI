using Api.Application.Exceptions;
using Api.Application.Features.Usuarios.Commands.RegistrarUsuario;
using Api.Application.Features.Usuarios.Queries.ObtenerUsuario;
using Api.Infrastructure.Api.Contracts;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Vogen;

namespace Api.Infrastructure.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
public sealed class UsuariosController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<UsuarioResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioResponse>> RegistrarAsync(
        RegistrarUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await sender.Send(
                new RegistrarUsuarioCommand(request.Nombre, request.Apellido, request.Email),
                cancellationToken);

            var response = new UsuarioResponse(
                resultado.Id,
                resultado.Nombre,
                resultado.Apellido,
                resultado.Email);

            return Created($"/api/usuarios/{response.Id}", response);
        }
        catch (ValidationException exception)
        {
            var errors = exception.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray());

            return BadRequest(new ValidationProblemDetails(errors)
            {
                Title = "Los datos del usuario no son válidos.",
            });
        }
        catch (ValueObjectValidationException exception)
        {
            return Problem(
                title: "Los datos del usuario no son válidos.",
                detail: exception.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
        catch (UsuarioEmailDuplicadoException exception)
        {
            return Problem(
                title: "El correo ya está registrado.",
                detail: exception.Message,
                statusCode: StatusCodes.Status409Conflict);
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<UsuarioResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioResponse>> ObtenerAsync(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await sender.Send(new ObtenerUsuarioQuery(id), cancellationToken);

        return resultado is null
            ? NotFound()
            : Ok(new UsuarioResponse(resultado.Id, resultado.Nombre, resultado.Apellido, resultado.Email));
    }
}
