using Application.Dtos.Transaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.InternalTransfer
{
    public class InternalTransferHistoryViewModel
    {
        public List<TransactionDto> Transactions { get; set; } = new();
        public Guid SelectedAccountId { get; set; }
    }
}
