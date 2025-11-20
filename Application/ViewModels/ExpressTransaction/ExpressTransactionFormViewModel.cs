
using System.ComponentModel.DataAnnotations;

namespace Application.ViewModels.ExpressTransaction
{
    public class ExpressTransactionFormViewModel
    {
        [Required(ErrorMessage = "La cuenta destino es obligatoria.")]
        [Display(Name = "Cuenta destino")]
        public string DestinationAccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero.")]
        [Display(Name = "Monto a transferir")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una cuenta de origen.")]
        [Display(Name = "Cuenta de origen")]
        public string OriginAccountNumber { get; set; } = string.Empty;

        public List<AccountOptionViewModelExpressTransaction> OriginAccounts { get; set; } = new();

        public bool ShowAccountWarning { get; set; } = false;
    }
}
