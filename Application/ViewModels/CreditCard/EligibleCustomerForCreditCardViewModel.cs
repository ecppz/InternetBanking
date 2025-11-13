
namespace Application.ViewModels.CreditCard
{
    public class EligibleCustomerForCreditCardViewModel
    {
        public required Guid UserId { get; set; }
        public string? DocumentNumber { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public decimal CurrentDebt { get; set; }
    }
}
