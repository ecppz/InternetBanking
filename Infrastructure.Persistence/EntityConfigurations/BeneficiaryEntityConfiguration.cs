using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Repositories
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
            builder.Property(b => b.Name).HasMaxLength(100).IsRequired();
            builder.Property(b => b.DocumentNumber).HasMaxLength(11).IsRequired();
            builder.Property(b => b.Email).HasMaxLength(100).IsRequired();
            #endregion

            #region indexs
            builder.HasIndex(b => new { b.OwnerUserId, b.DocumentNumber }).IsUnique();
            #endregion
        }
    }
}
