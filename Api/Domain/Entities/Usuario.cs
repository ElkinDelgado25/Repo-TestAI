using Api.Domain.Common;
using Api.Domain.ValueObjects;

namespace Api.Domain.Entities;

public sealed class Usuario : Entity<UsuarioId>
{
    public Usuario(UsuarioId id, Nombre nombre, Apellido apellido, Email email)
        : base(id)
    {
        Nombre = nombre;
        Apellido = apellido;
        Email = email;
    }

    public Nombre Nombre { get; }

    public Apellido Apellido { get; }

    public Email Email { get; }
}
