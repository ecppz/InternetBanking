using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Transaction
{
    public class DepositConfirmationViewModel
    {
        public string DestinationAccountNumber { get; set; } = string.Empty;
        public string DestinationOwnerFullName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
