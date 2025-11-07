using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Repositories
{
    public class CreditCardEntityConfiguration : IEntityTypeConfiguration<CreditCard>
    {
        public void Configure(EntityTypeBuilder<CreditCard> builder)
        {
            #region basic configuration
            builder.ToTable("CreditCards");
            builder.HasKey(cc => cc.Id);
            #endregion

            #region property configurations
            builder.Property(cc => cc.CardNumber).HasMaxLength(16).IsRequired();
            builder.Property(cc => cc.ExpirationDate).HasMaxLength(5).IsRequired();
            builder.Property(cc => cc.CvcHash).HasMaxLength(64).IsRequired();
            builder.Property(cc => cc.CreditLimit).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(cc => cc.CurrentDebt).HasColumnType("decimal(18,2)");
            #endregion

            #region indexs
            builder.HasIndex(cc => cc.CardNumber).IsUnique();
            #endregion
        }
    }
}
