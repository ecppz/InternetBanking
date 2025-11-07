using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Repositories
{
    public class LoanInstallmentEntityConfiguration : IEntityTypeConfiguration<LoanInstallment>
    {
        public void Configure(EntityTypeBuilder<LoanInstallment> builder)
        {
            #region basic configuration
            builder.ToTable("LoanInstallments");
            builder.HasKey(li => li.Id);
            #endregion

            #region property configurations
            builder.Property(li => li.Amount).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(li => li.DueDate).IsRequired();
            #endregion

            #region indexs
            builder.HasIndex(li => new { li.LoanId, li.DueDate }).IsUnique();
            #endregion
        }
    }
}
