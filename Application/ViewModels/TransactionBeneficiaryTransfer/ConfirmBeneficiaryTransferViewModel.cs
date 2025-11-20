using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.TransactionBeneficiaryTransfer
{
    public class ConfirmBeneficiaryTransferViewModel
    {
        public Guid OriginAccountId { get; set; }
        public string OriginAccountNumber { get; set; } = null!;
        public string BeneficiaryAccountNumber { get; set; } = null!;
        public string BeneficiaryFullName { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
