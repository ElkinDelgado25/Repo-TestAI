using Api.Application.Abstractions.Persistence;
using Api.Application.Exceptions;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Features.Usuarios.Commands.RegistrarUsuario;

public sealed class RegistrarUsuarioHandler(IApplicationDbContext dbContext)
    : IRequestHandler<RegistrarUsuarioCommand, RegistrarUsuarioResult>
{
    public async Task<RegistrarUsuarioResult> Handle(
        RegistrarUsuarioCommand command,
        CancellationToken cancellationToken)
    {
        var email = Email.From(command.Email);

        if (await dbContext.Usuarios.AnyAsync(usuario => usuario.Email == email, cancellationToken))
        {
            throw new UsuarioEmailDuplicadoException(email.Value);
        }

        var usuario = new Usuario(
            UsuarioId.From(Guid.NewGuid()),
            Nombre.From(command.Nombre),
            Apellido.From(command.Apellido),
            email);

        dbContext.Usuarios.Add(usuario);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new RegistrarUsuarioResult(
            usuario.Id.Value,
            usuario.Nombre.Value,
            usuario.Apellido.Value,
            usuario.Email.Value);
    }
}
