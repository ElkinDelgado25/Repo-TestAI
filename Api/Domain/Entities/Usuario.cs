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

    public Nombre Nombre { get; private set; }

    public Apellido Apellido { get; private set; }

    public Email Email { get; private set; }

    public bool EstaEliminado { get; private set; }

    public DateTimeOffset? EliminadoEn { get; private set; }

    public void Actualizar(Nombre nombre, Apellido apellido, Email email)
    {
        if (EstaEliminado)
        {
            throw new InvalidOperationException("No se puede actualizar un usuario eliminado.");
        }

        Nombre = nombre;
        Apellido = apellido;
        Email = email;
    }

    public void Eliminar(DateTimeOffset eliminadoEn)
    {
        if (EstaEliminado)
        {
            return;
        }

        EstaEliminado = true;
        EliminadoEn = eliminadoEn;
    }
}
