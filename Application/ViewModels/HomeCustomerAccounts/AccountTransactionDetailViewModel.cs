using Application.ViewModels.Transaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.HomeCustomerAccounts
{
    public class AccountTransactionDetailViewModel
    {
        public string AccountNumber { get; set; } = null!;
        public List<TransactionDetailViewModel> Transactions { get; set; } = new();
    }
}
