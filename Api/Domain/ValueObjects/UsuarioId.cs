using Vogen;

namespace Api.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public partial struct UsuarioId
{
    private static Validation Validate(Guid value)
    {
        return value != Guid.Empty
            ? Validation.Ok
            : Validation.Invalid("El identificador del usuario es obligatorio.");
    }
}
