using FluentValidation;

namespace Api.Application.Features.Usuarios.Queries.ObtenerUsuario;

public sealed class ObtenerUsuarioValidator : AbstractValidator<ObtenerUsuarioQuery>
{
    public ObtenerUsuarioValidator()
    {
        RuleFor(query => query.Id).NotEqual(Guid.Empty);
    }
}
