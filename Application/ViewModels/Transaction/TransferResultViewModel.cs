using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Transaction
{
    public class TransferResultViewModel
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public string RedirectUrl { get; set; } = "/Cashier/Home";
    }
}
