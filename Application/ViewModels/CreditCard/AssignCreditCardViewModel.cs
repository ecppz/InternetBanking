using System.ComponentModel.DataAnnotations;

namespace Application.ViewModels.CreditCard
{
    public class AssignCreditCardViewModel
    {
        public Guid UserId { get; set; }
        public required Guid AdminUserId { get; set; }
        public string? DocumentNumber { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }

        [Required(ErrorMessage = "El límite de crédito es obligatorio.")]
        [Range(1, double.MaxValue, ErrorMessage = "El límite debe ser mayor a cero.")]
        public decimal CreditLimit { get; set; }
    }
}

