using System.ComponentModel.DataAnnotations;

namespace Application.ViewModels.CreditCardTransaction
{
    public class CreditCardPaymentViewModel
    {
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Debe ingresar número de cuenta")]
        public required string AccountNumber { get; set; }

        [Required(ErrorMessage = "Debe ingresar una tarjeta de credito")]
        public required string CreditCardNumber { get; set; }

        [Required(ErrorMessage = "Debe ingresar un monto")]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public required decimal Amount { get; set; }
        
    }
}