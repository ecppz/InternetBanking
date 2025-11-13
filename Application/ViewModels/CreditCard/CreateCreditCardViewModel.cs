using System.ComponentModel.DataAnnotations;

namespace Application.ViewModels.CreditCard
{
    public class CreateCreditCardViewModel
    {
        public Guid UserId { get; set; }
        public required Guid AdminUserId { get; set; }

        [Required(ErrorMessage = "El límite de crédito es obligatorio.")]
        [Range(1, double.MaxValue, ErrorMessage = "El límite debe ser mayor a cero.")]
        public decimal CreditLimit { get; set; }
    }
}
