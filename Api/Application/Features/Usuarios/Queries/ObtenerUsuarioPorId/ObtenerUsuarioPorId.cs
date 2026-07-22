using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Api.Infrastructure.Data;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Features.Usuarios.Queries.ObtenerUsuarioPorId;

public sealed record ObtenerUsuarioPorIdQuery(Guid Id) : IRequest<UsuarioPorIdDto?>;

public sealed record UsuarioPorIdDto(Guid Id, string Nombre, string Apellido, string Email);

public sealed class ObtenerUsuarioPorIdHandler(ApiDbContext dbContext)
    : IRequestHandler<ObtenerUsuarioPorIdQuery, UsuarioPorIdDto?>
{
    public async Task<UsuarioPorIdDto?> Handle(
        ObtenerUsuarioPorIdQuery query,
        CancellationToken cancellationToken)
    {
        var id = UsuarioId.From(query.Id);
        var usuario = await dbContext.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        return usuario?.ToUsuarioPorIdDto();
    }
}

public sealed class ObtenerUsuarioPorIdValidator : AbstractValidator<ObtenerUsuarioPorIdQuery>
{
    public ObtenerUsuarioPorIdValidator()
    {
        RuleFor(query => query.Id)
            .NotEmpty();
    }
}

internal static class ObtenerUsuarioPorIdMappings
{
    internal static UsuarioPorIdDto ToUsuarioPorIdDto(this Usuario usuario)
    {
        return new UsuarioPorIdDto(
            usuario.Id.Value,
            usuario.Nombre.Value,
            usuario.Apellido.Value,
            usuario.Email.Value);
    }
}

[ApiController]
[Route("api/usuarios")]
public sealed class ObtenerUsuarioPorIdController(ISender sender) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType<UsuarioPorIdDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UsuarioPorIdDto>> ObtenerAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var usuario = await sender.Send(
                new ObtenerUsuarioPorIdQuery(id),
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
                Title = "El identificador del usuario no es válido.",
            });
        }
    }
}
