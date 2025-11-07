using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Repositories
{
    public class TransactionEntityConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            #region basic configuration
            builder.ToTable("Transactions");
            builder.HasKey(t => t.Id);
            #endregion

            #region property configurations
            builder.Property(t => t.Amount).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(t => t.Date).IsRequired();
            builder.Property(t => t.Type).HasConversion<string>().IsRequired();
            #endregion

            #region indexs
            builder.HasIndex(t => t.Date);
            #endregion
        }
    }
}
