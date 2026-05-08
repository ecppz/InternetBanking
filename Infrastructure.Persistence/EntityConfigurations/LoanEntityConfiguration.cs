using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.EntityConfigurations
{
    public class LoanEntityConfiguration : IEntityTypeConfiguration<Loan>
    {
        public void Configure(EntityTypeBuilder<Loan> builder)
        {
            #region basic configuration
            builder.ToTable("Loans");
            builder.HasKey(l => l.Id);
            #endregion

            #region property configurations
            builder.Property(l => l.LoanNumber).HasMaxLength(20).IsRequired();
            builder.Property(l => l.Amount).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(l => l.AnnualInterestRate).HasColumnType("decimal(5,2)").IsRequired();
            #endregion

            #region indexs
            builder.HasIndex(l => l.LoanNumber).IsUnique();
            #endregion
        }
    }
}
