using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Transaction
{
    public class ThirdPartyTransferViewModel
    {
        [Required(ErrorMessage = "Debes ingresar el número de cuenta origen.")]
        public string OriginAccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debes ingresar el monto.")]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Debes ingresar el número de cuenta destino.")]
        public string DestinationAccountNumber { get; set; } = string.Empty;

        // Mensajes de error visuales
        public string? ErrorMessage { get; set; }
    }
}
