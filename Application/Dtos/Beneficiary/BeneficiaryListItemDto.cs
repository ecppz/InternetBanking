using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Beneficiary
{
    public class BeneficiaryListItemDto
    {
        public Guid Id { get; set; }
        public string BeneficiaryAccountNumber { get; set; } = null!;
        public string FullName => $"{Name} {LastName}";
        public string Name { get; set; } = null!;
        public string LastName { get; set; } = null!;
    }
}
