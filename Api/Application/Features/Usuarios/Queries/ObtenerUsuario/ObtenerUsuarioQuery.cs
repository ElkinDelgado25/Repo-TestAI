using MediatR;

namespace Api.Application.Features.Usuarios.Queries.ObtenerUsuario;

public sealed record ObtenerUsuarioQuery(Guid Id) : IRequest<ObtenerUsuarioResult?>;
