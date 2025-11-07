using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Repositories
{
    public class CreditCardTransactionEntityConfiguration : IEntityTypeConfiguration<CreditCardTransaction>
    {
        public void Configure(EntityTypeBuilder<CreditCardTransaction> builder)
        {
            #region basic configuration
            builder.ToTable("CreditCardTransactions");
            builder.HasKey(ct => ct.Id);
            #endregion

            #region property configurations
            builder.Property(ct => ct.Date).IsRequired();
            builder.Property(ct => ct.Amount).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(ct => ct.TransactionOrigin).HasMaxLength(100).IsRequired();
            builder.Property(ct => ct.Status).HasConversion<string>().IsRequired();
            #endregion

            #region indexs
            builder.HasIndex(ct => ct.Date);
            #endregion
        }
    }
}
