using Api.Domain.ValueObjects;
using Api.Infrastructure.Data;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Features.Usuarios.Commands.EliminarUsuario;

public sealed record EliminarUsuarioCommand(Guid Id) : IRequest<bool>;

public sealed class EliminarUsuarioHandler(ApiDbContext dbContext)
    : IRequestHandler<EliminarUsuarioCommand, bool>
{
    public async Task<bool> Handle(
        EliminarUsuarioCommand command,
        CancellationToken cancellationToken)
    {
        var id = UsuarioId.From(command.Id);
        var usuario = await dbContext.Usuarios
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        if (usuario is null)
        {
            return false;
        }

        usuario.Eliminar(DateTimeOffset.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}

public sealed class EliminarUsuarioValidator : AbstractValidator<EliminarUsuarioCommand>
{
    public EliminarUsuarioValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();
    }
}

[ApiController]
[Route("api/usuarios")]
public sealed class EliminarUsuarioController(ISender sender) : ControllerBase
{
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EliminarAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var eliminado = await sender.Send(new EliminarUsuarioCommand(id), cancellationToken);

            return eliminado ? NoContent() : NotFound();
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
