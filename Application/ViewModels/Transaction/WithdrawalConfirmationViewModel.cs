using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Transaction
{
    public class WithdrawalConfirmationViewModel
    {
        public string OriginAccountNumber { get; set; } = string.Empty;
        public string OriginOwnerFullName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
