using Application.Dtos.Transaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.SavingsAccount
{
    public class SavingsAccountDetailDto
    {
        public required Guid Id { get; set; }
        public required string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public bool IsPrimary { get; set; }

        public required Guid UserId { get; set; }
        public required string OwnerFullName { get; set; }
        public required string DocumentNumber { get; set; }

        public List<TransactionDto> Transactions { get; set; } = new();
    }
}
