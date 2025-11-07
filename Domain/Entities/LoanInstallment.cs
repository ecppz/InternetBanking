namespace Domain.Entities
{
    public class LoanInstallment
    {
        public Guid Id { get; set; }
        public Guid LoanId { get; set; }
        public required DateTime DueDate { get; set; }
        public required decimal Amount { get; set; }
        public bool IsPaid { get; set; }
        public bool IsLate { get; set; }
    }

}
