using Api.Domain.Entities;
using Api.Infrastructure.Data;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Features.Usuarios.Queries.ObtenerUsuarios;

public sealed record ObtenerUsuariosQuery(int Pagina = 1, int TamanoPagina = 10)
    : IRequest<UsuariosPaginadosDto>;

public sealed record UsuarioDto(Guid Id, string Nombre, string Apellido, string Email);

public sealed record UsuariosPaginadosDto(
    IReadOnlyList<UsuarioDto> Usuarios,
    int Pagina,
    int TamanoPagina,
    int TotalRegistros,
    int TotalPaginas);

public sealed class ObtenerUsuariosHandler(ApiDbContext dbContext)
    : IRequestHandler<ObtenerUsuariosQuery, UsuariosPaginadosDto>
{
    public async Task<UsuariosPaginadosDto> Handle(
        ObtenerUsuariosQuery query,
        CancellationToken cancellationToken)
    {
        var totalRegistros = await dbContext.Usuarios.CountAsync(cancellationToken);
        var usuarios = await dbContext.Usuarios
            .AsNoTracking()
            .OrderBy(entity => entity.Apellido)
            .ThenBy(entity => entity.Nombre)
            .Skip((query.Pagina - 1) * query.TamanoPagina)
            .Take(query.TamanoPagina)
            .Select(usuario => usuario.ToUsuarioDto())
            .ToListAsync(cancellationToken);

        var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)query.TamanoPagina);

        return new UsuariosPaginadosDto(
            usuarios,
            query.Pagina,
            query.TamanoPagina,
            totalRegistros,
            totalPaginas);
    }
}

public sealed class ObtenerUsuariosValidator : AbstractValidator<ObtenerUsuariosQuery>
{
    public ObtenerUsuariosValidator()
    {
        RuleFor(query => query.Pagina)
            .GreaterThanOrEqualTo(1);

        RuleFor(query => query.TamanoPagina)
            .InclusiveBetween(1, 100);
    }
}

internal static class ObtenerUsuariosMappings
{
    internal static UsuarioDto ToUsuarioDto(this Usuario usuario)
    {
        return new UsuarioDto(
            usuario.Id.Value,
            usuario.Nombre.Value,
            usuario.Apellido.Value,
            usuario.Email.Value);
    }
}

[ApiController]
[Route("api/usuarios")]
public sealed class ObtenerUsuariosController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<UsuariosPaginadosDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UsuariosPaginadosDto>> ObtenerAsync(
        [FromQuery] ObtenerUsuariosQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var usuarios = await sender.Send(query, cancellationToken);

            return Ok(usuarios);
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
                Title = "Los parámetros de paginación no son válidos.",
            });
        }
    }
}
