using Api.Application.Exceptions;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Api.Infrastructure.Data;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vogen;

namespace Api.Application.Features.Usuarios.Commands.ActualizarUsuario;

public sealed record ActualizarUsuarioCommand(Guid Id, string Nombre, string Apellido, string Email)
    : IRequest<ActualizarUsuarioDto?>;

public sealed record ActualizarUsuarioDto(Guid Id, string Nombre, string Apellido, string Email);

public sealed record ActualizarUsuarioRequest(string Nombre, string Apellido, string Email);

public sealed class ActualizarUsuarioHandler(ApiDbContext dbContext)
    : IRequestHandler<ActualizarUsuarioCommand, ActualizarUsuarioDto?>
{
    public async Task<ActualizarUsuarioDto?> Handle(
        ActualizarUsuarioCommand command,
        CancellationToken cancellationToken)
    {
        var id = UsuarioId.From(command.Id);
        var usuario = await dbContext.Usuarios
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        if (usuario is null)
        {
            return null;
        }

        var email = Email.From(command.Email);
        var existeOtroUsuarioConEmail = await dbContext.Usuarios
            .IgnoreQueryFilters()
            .AnyAsync(
                entity => entity.Id != id && entity.Email == email,
                cancellationToken);

        if (existeOtroUsuarioConEmail)
        {
            throw new UsuarioEmailDuplicadoException(email.Value);
        }

        usuario.Actualizar(
            Nombre.From(command.Nombre),
            Apellido.From(command.Apellido),
            email);
        await dbContext.SaveChangesAsync(cancellationToken);

        return usuario.ToActualizarUsuarioDto();
    }
}

public sealed class ActualizarUsuarioValidator : AbstractValidator<ActualizarUsuarioCommand>
{
    public ActualizarUsuarioValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();

        RuleFor(command => command.Nombre)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Apellido)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(254);
    }
}

internal static class ActualizarUsuarioMappings
{
    internal static ActualizarUsuarioDto ToActualizarUsuarioDto(this Usuario usuario)
    {
        return new ActualizarUsuarioDto(
            usuario.Id.Value,
            usuario.Nombre.Value,
            usuario.Apellido.Value,
            usuario.Email.Value);
    }
}

[ApiController]
[Route("api/usuarios")]
public sealed class ActualizarUsuarioController(ISender sender) : ControllerBase
{
    [HttpPut("{id:guid}")]
    [ProducesResponseType<ActualizarUsuarioDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ActualizarUsuarioDto>> ActualizarAsync(
        Guid id,
        ActualizarUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var usuario = await sender.Send(
                new ActualizarUsuarioCommand(id, request.Nombre, request.Apellido, request.Email),
                cancellationToken);

            return usuario is null ? NotFound() : Ok(usuario);
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
}
