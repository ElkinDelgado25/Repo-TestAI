using FluentValidation;

namespace Api.Application.Features.Usuarios.Commands.RegistrarUsuario;

public sealed class RegistrarUsuarioValidator : AbstractValidator<RegistrarUsuarioCommand>
{
    public RegistrarUsuarioValidator()
    {
        RuleFor(command => command.Nombre)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Apellido)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(254);
    }
}
