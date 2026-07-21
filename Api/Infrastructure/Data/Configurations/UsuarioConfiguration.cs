using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Infrastructure.Data.Configurations;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(usuario => usuario.Id);

        builder.Property(usuario => usuario.Id)
            .HasConversion(new UsuarioId.EfCoreValueConverter())
            .ValueGeneratedNever();

        builder.Property(usuario => usuario.Nombre)
            .HasConversion(new Nombre.EfCoreValueConverter())
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(usuario => usuario.Apellido)
            .HasConversion(new Apellido.EfCoreValueConverter())
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(usuario => usuario.Email)
            .HasConversion(new Email.EfCoreValueConverter())
            .HasMaxLength(254)
            .IsRequired();

        builder.HasIndex(usuario => usuario.Email)
            .IsUnique();
    }
}
