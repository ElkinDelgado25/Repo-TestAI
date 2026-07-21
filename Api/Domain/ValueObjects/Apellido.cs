using Vogen;

namespace Api.Domain.ValueObjects;

[ValueObject<string>(conversions: Conversions.Default | Conversions.EfCoreValueConverter)]
public partial struct Apellido;
