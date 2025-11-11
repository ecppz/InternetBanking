using Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.SavingsAccount
{
    public class CancelSavingsAccountViewModel
    {
        public Guid AccountId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Balance { get; set; }

        public bool IsActive { get; set; }

        public SavingsAccountStatus Estado => IsActive ? SavingsAccountStatus.Activa : SavingsAccountStatus.Cancelada;

        public string EstadoVisual => Estado.ToString();
    }
}
