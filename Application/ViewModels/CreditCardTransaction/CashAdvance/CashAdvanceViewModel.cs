using System.ComponentModel.DataAnnotations;

namespace Application.ViewModels.CreditCardTransaction.CashAdvance
{
    public class CashAdvanceViewModel
    {
        public Guid UserId { get; set; }
        [Required(ErrorMessage = "Debe seleccionar una tarjeta.")]
        public Guid CreditCardId { get; set; }
        [Required(ErrorMessage = "Debe seleccionar una cuenta.")]
        public Guid SavingsAccountId { get; set; }
        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "El monto del avance debe ser mayor a cero.")]
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
