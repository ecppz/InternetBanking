using Application.Dtos.SavingsAccount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.SavingsAccount
{
    public class SavingsAccountDetailViewModel
    {
        public required SavingsAccountDetailDto Account { get; set; }
    }
}
