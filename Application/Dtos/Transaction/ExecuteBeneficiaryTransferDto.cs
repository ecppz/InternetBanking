using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Transaction
{
    public class ExecuteBeneficiaryTransferDto
    {
        public required string OriginAccountNumber { get; set; }
        public required string BeneficiaryAccountNumber { get; set; }
        public required decimal Amount { get; set; }
        public required DateTime Timestamp { get; set; }
        public required string BeneficiaryFullName { get; set; }
    }
}
