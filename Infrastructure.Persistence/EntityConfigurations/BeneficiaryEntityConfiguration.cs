using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations
{
    public class BeneficiaryEntityConfiguration : IEntityTypeConfiguration<Beneficiary>
    {
        public void Configure(EntityTypeBuilder<Beneficiary> builder)
        {
            #region basic configuration
            builder.ToTable("Beneficiaries");
            builder.HasKey(b => b.Id);
            #endregion

            #region property configurations
            // FK del Dueño de la lista (Cliente conectado)
            builder.Property(b => b.OwnerUserId)
                   .IsRequired();

            // Campo CRÍTICO: Número de Cuenta del Beneficiario
            builder.Property(b => b.BeneficiaryAccountNumber)
                   .HasMaxLength(20) // Asumiendo que las cuentas tienen 20 dígitos o menos
                   .IsRequired();

            // ID del usuario al que pertenece la cuenta Beneficiaria
            builder.Property(b => b.BeneficiaryUserId)
                   .IsRequired();

            // Nombre y Apellido del titular (para el listado)
            builder.Property(b => b.Name)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(b => b.LastName)
                   .HasMaxLength(100)
                   .IsRequired();

            // **CAMPOS OBSOLETOS DE TU PROPUESTA ANTERIOR (DEBEN ELIMINARSE O CAMBIARSE):**
            // builder.Property(b => b.DocumentNumber)...
            // builder.Property(b => b.Email)...
            #endregion

            #region indexs
            // RESTRICCIÓN CLAVE: Un cliente (OwnerUserId) solo puede guardar una vez el mismo número de cuenta (BeneficiaryAccountNumber).
            builder.HasIndex(b => new { b.OwnerUserId, b.BeneficiaryAccountNumber })
                   .IsUnique();
            #endregion
        }
    }
}
