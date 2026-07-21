using Api.Application.Abstractions.Persistence;
using Api.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Features.Usuarios.Queries.ObtenerUsuario;

public sealed class ObtenerUsuarioHandler(IApplicationDbContext dbContext)
    : IRequestHandler<ObtenerUsuarioQuery, ObtenerUsuarioResult?>
{
    public async Task<ObtenerUsuarioResult?> Handle(
        ObtenerUsuarioQuery query,
        CancellationToken cancellationToken)
    {
        var usuarioId = UsuarioId.From(query.Id);
        var usuario = await dbContext.Usuarios
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.Id == usuarioId, cancellationToken);

        return usuario is null
            ? null
            : new ObtenerUsuarioResult(
                usuario.Id.Value,
                usuario.Nombre.Value,
                usuario.Apellido.Value,
                usuario.Email.Value);
    }
}
