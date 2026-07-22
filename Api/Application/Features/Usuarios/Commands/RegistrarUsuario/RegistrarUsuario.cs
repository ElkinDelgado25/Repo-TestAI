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

namespace Api.Application.Features.Usuarios.Commands.RegistrarUsuario;

public sealed record RegistrarUsuarioCommand(string Nombre, string Apellido, string Email)
    : IRequest<RegistrarUsuarioDto>;

public sealed record RegistrarUsuarioDto(Guid Id, string Nombre, string Apellido, string Email);

public sealed record RegistrarUsuarioRequest(string Nombre, string Apellido, string Email);

public sealed class RegistrarUsuarioHandler(ApiDbContext dbContext)
    : IRequestHandler<RegistrarUsuarioCommand, RegistrarUsuarioDto>
{
    public async Task<RegistrarUsuarioDto> Handle(
        RegistrarUsuarioCommand command,
        CancellationToken cancellationToken
    )
    {
        var email = Email.From(command.Email);

        if (await dbContext.Usuarios
            .IgnoreQueryFilters()
            .AnyAsync(usuario => usuario.Email == email, cancellationToken))
        {
            throw new UsuarioEmailDuplicadoException(email.Value);
        }

        var usuario = new Usuario(
            UsuarioId.From(Guid.NewGuid()),
            Nombre.From(command.Nombre),
            Apellido.From(command.Apellido),
            email
        );

        dbContext.Usuarios.Add(usuario);
        await dbContext.SaveChangesAsync(cancellationToken);

        return usuario.ToRegistrarUsuarioDto();
    }
}

public sealed class RegistrarUsuarioValidator : AbstractValidator<RegistrarUsuarioCommand>
{
    public RegistrarUsuarioValidator()
    {
        RuleFor(command => command.Nombre).NotEmpty().MaximumLength(100);

        RuleFor(command => command.Apellido).NotEmpty().MaximumLength(100);

        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(254);
    }
}

internal static class RegistrarUsuarioMappings
{
    internal static RegistrarUsuarioDto ToRegistrarUsuarioDto(this Usuario usuario)
    {
        return new RegistrarUsuarioDto(
            usuario.Id.Value,
            usuario.Nombre.Value,
            usuario.Apellido.Value,
            usuario.Email.Value
        );
    }
}

[ApiController]
[Route("api/usuarios")]
public sealed class RegistrarUsuarioController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<RegistrarUsuarioDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegistrarUsuarioDto>> RegistrarAsync(
        RegistrarUsuarioRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var resultado = await sender.Send(
                new RegistrarUsuarioCommand(request.Nombre, request.Apellido, request.Email),
                cancellationToken
            );

            return Created($"/api/usuarios/{resultado.Id}", resultado);
        }
        catch (ValidationException exception)
        {
            var errors = exception
                .Errors.GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray()
                );

            return BadRequest(
                new ValidationProblemDetails(errors)
                {
                    Title = "Los datos del usuario no son válidos.",
                }
            );
        }
        catch (ValueObjectValidationException exception)
        {
            return Problem(
                title: "Los datos del usuario no son válidos.",
                detail: exception.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (UsuarioEmailDuplicadoException exception)
        {
            return Problem(
                title: "El correo ya está registrado.",
                detail: exception.Message,
                statusCode: StatusCodes.Status409Conflict
            );
        }
    }
}
