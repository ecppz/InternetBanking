using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Transaction
{
    public class ConfirmThirdPartyTransferViewModel
    {
        public string OriginAccountNumber { get; set; } = string.Empty;

        public string DestinationAccountNumber { get; set; } = string.Empty;

        public string DestinationFullName { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
