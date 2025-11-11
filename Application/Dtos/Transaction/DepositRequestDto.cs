using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Transaction
{
    public class DepositRequestDto
    {
        public required string DestinationAccountNumber { get; set; }
        public required decimal Amount { get; set; }
    }
}
