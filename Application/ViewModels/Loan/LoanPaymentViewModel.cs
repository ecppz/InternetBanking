using System.ComponentModel.DataAnnotations;

namespace Application.ViewModels.Loan
{
    public class LoanPaymentViewModel
    {
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una cuenta de origen")]
        public required string OriginAccountNumber { get; set; }  

        [Required(ErrorMessage = "Debe ingresar un monto")]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un préstamo")]
        public required string LoanNumber { get; set; }
    }
}
