using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Beneficiary
{
    public class CreateBeneficiaryDto
    {
        [Required(ErrorMessage = "El número de cuenta es obligatorio.")]
        public string BeneficiaryAccountNumber { get; set; } = null!;

        public Guid OwnerUserId { get; set; } // ← Usuario que agrega el beneficiario
        public Guid BeneficiaryUserId { get; set; } // ← Usuario dueño de la cuenta ingresada
    }
}
