using Vogen;

namespace Api.Domain.ValueObjects;

[ValueObject<string>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public partial struct Email
{
    private static string NormalizeInput(string input)
    {
        return input.Trim().ToLowerInvariant();
    }

    private static Validation Validate(string input)
    {
        var posicionArroba = input.IndexOf('@');
        var dominio = posicionArroba >= 0 ? input[(posicionArroba + 1)..] : string.Empty;

        var esValido = !string.IsNullOrWhiteSpace(input)
            && !input.Any(char.IsWhiteSpace)
            && posicionArroba > 0
            && posicionArroba == input.LastIndexOf('@')
            && dominio.Length > 2
            && dominio.Contains('.')
            && !dominio.StartsWith('.')
            && !dominio.EndsWith('.');

        return esValido
            ? Validation.Ok
            : Validation.Invalid("El correo electrónico no tiene un formato válido.");
    }
}
