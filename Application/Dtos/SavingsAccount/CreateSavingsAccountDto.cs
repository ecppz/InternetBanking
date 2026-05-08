using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.SavingsAccount
{
    public class CreateSavingsAccountDto
    {
        public required Guid UserId { get; set; }
        public decimal InitialBalance { get; set; }
        public bool IsPrimary { get; set; } = false;
    }
}
