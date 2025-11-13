using Domain.Common.Enums;

namespace Application.ViewModels.CreditCard
{
    public class CreditCardDisplayViewModel
    {
        public Guid Id { get; set; }
        public required string CardNumber { get; set; }
        public string? CustomerFullName { get; set; }
        public string? DocumentNumber { get; set; }
        public required decimal CreditLimit { get; set; }
        public DateTime ExpirationDate { get; set; }
        public required decimal CurrentDebt { get; set; }
        public CreditCardStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
