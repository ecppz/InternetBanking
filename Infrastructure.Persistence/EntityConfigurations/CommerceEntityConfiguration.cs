using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations
{
    public class CommerceEntityConfiguration : IEntityTypeConfiguration<Commerce>
    {
        public void Configure(EntityTypeBuilder<Commerce> builder)
        {
            #region basic configuration
            builder.ToTable("Commerces");
            builder.HasKey(c => c.Id);
            #endregion

            #region property configurations
            builder.Property(c => c.Name)
                   .HasMaxLength(150)
                   .IsRequired();

            builder.Property(c => c.Rnc)
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(c => c.Address)
                   .HasMaxLength(250)
                   .IsRequired();

            builder.Property(c => c.Status)
                   .IsRequired();

            builder.Property(c => c.CreatedAt)
                   .IsRequired();
            #endregion

            #region indexes
            builder.HasIndex(c => c.Rnc)
                   .IsUnique();
            #endregion
        }
    }
}