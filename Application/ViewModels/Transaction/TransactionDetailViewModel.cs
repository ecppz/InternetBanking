using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Transaction
{
    public class TransactionDetailViewModel
    {
        public required TransactionViewModel Transaction { get; set; }

        // Datos enriquecidos opcionales
        public string? OriginAccountNumber { get; set; }
        public string? DestinationAccountNumber { get; set; }
        public string? OwnerFullName { get; set; }
    }
}
