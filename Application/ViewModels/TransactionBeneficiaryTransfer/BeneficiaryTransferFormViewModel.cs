using Application.Dtos.Beneficiary;
using Application.ViewModels.ExpressTransaction;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.TransactionBeneficiaryTransfer
{
    public class BeneficiaryTransferFormViewModel
    {
        [Required]
        public string BeneficiaryAccountNumber { get; set; } = null!;

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero.")]
        public decimal Amount { get; set; }

        [Required]
        public Guid OriginAccountId { get; set; }

        // Para poblar el selector de beneficiarios
        public List<SelectListItem> Beneficiaries { get; set; } = new();

        // Para poblar el selector de cuentas activas
        public List<AccountOptionViewModelBeneficiary> OriginAccounts { get; set; } = new();

       
    }
}
