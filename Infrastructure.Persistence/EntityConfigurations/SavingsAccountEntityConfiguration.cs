using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Repositories
{
    public class SavingsAccountEntityConfiguration : IEntityTypeConfiguration<SavingsAccount>
    {
        public void Configure(EntityTypeBuilder<SavingsAccount> builder)
        {
            #region basic configuration
            builder.ToTable("SavingsAccounts");
            builder.HasKey(sa => sa.Id);
            #endregion

            #region property configurations
            builder.Property(sa => sa.AccountNumber).HasMaxLength(9).IsRequired();
            builder.Property(sa => sa.Balance).HasColumnType("decimal(18,2)");
            #endregion

            #region indexs
            builder.HasIndex(sa => sa.AccountNumber).IsUnique();
            #endregion
        }
    }
}
