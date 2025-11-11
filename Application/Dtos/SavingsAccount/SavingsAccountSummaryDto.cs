using Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.SavingsAccount
{
    public class SavingsAccountSummaryDto
    {
        public required Guid Id { get; set; }
        public required string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public bool IsPrimary { get; set; }
        public required string OwnerFullName { get; set; }
        public required string DocumentNumber { get; set; }

        public Guid UserId { get; set; }

        public bool IsActive => Estado == SavingsAccountStatus.Activa;
        public SavingsAccountStatus Estado { get; set; }

    }
}
