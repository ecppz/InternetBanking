using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Beneficiary
{
    public class CreateBeneficiaryViewModel
    {
        [Required(ErrorMessage = "Debe ingresar el número de cuenta.")]
        public string BeneficiaryAccountNumber { get; set; } = null!;
    }
}
