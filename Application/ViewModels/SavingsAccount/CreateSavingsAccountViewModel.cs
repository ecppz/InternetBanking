using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.SavingsAccount
{
    public class CreateSavingsAccountViewModel
    {
        public required Guid UserId { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "El balance inicial no puede ser negativo.")]
        public decimal InitialBalance { get; set; }

        public bool IsPrimary { get; set; } = false;
    }
}
