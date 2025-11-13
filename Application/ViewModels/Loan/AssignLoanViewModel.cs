using System.ComponentModel.DataAnnotations;

namespace Application.ViewModels.Loan
{
    public class AssignLoanViewModel
    {
        public Guid UserId { get; set; }
        public string? DocumentNumber { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; } 

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(1, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "La tasa de interés es obligatoria")]
        [Range(0.01, 100, ErrorMessage = "La tasa debe estar entre 0.01 y 100")]
        public decimal AnnualRate { get; set; }

        [Required(ErrorMessage = "El plazo es obligatorio")]
        [Range(6, 60, ErrorMessage = "El plazo debe estar entre 6 y 60 meses")]
        public int Months { get; set; }
    }
}
