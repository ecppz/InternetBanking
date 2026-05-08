using Application.Dtos.SavingsAccount;

namespace Application.ViewModels.SavingsAccount
{
    public class SavingsAccountListViewModel
    {
        public List<SavingsAccountSummaryDto> Accounts { get; set; } = new();

        // Filtros aplicados
        public string? DocumentNumber { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsPrimary { get; set; }

        // Paginación
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;


    }
}
