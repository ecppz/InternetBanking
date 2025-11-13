using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.SavingsAccount
{
    public class SavingsAccountFilterViewModel
    {
        public string? DocumentNumber { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsPrimary { get; set; }
    }
}
