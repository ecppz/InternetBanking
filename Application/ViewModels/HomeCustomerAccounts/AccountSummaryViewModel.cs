using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.HomeCustomerAccounts
{
    public class AccountSummaryViewModel
    {
        public Guid Id { get; set; }
        public string AccountNumber { get; set; } = null!;
        public decimal Balance { get; set; }
        public bool IsPrimary { get; set; }
    }
}
