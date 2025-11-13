using System.ComponentModel.DataAnnotations;

namespace Application.ViewModels.Loan
{
    public class EditLoanViewModel
    {
        public Guid Id { get; set; }
        [Required]
        [Range(0.01, 100.00, ErrorMessage = "La tasa debe estar entre 0.01 y 100")]
        public decimal AnnualInterestRate { get; set; }
    }

}
