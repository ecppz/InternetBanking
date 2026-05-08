using Application.Dtos.Transaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Transaction
{
    public class TransactionHistoryViewModel
    {
        public string AccountNumber { get; set; } = string.Empty;

        public List<TransactionDto> Transactions { get; set; } = new();
    }
}
