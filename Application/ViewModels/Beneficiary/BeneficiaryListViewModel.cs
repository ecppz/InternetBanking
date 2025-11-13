using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Beneficiary
{
    public class BeneficiaryListViewModel
    {
        public List<BeneficiaryViewModel> Beneficiaries { get; set; } = new();
        public CreateBeneficiaryViewModel NewBeneficiary { get; set; } = new();
    }
}
