using Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Transaction
{
    public class TransactionListViewModel
    {
        public List<TransactionViewModel> Transactions { get; set; } = new();

        // Filtros opcionales
        public Guid? AccountId { get; set; }
        public string? DocumentNumber { get; set; }
        public TransactionType? Type { get; set; }

        // Rango de fechas
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Paginación
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;
    }
}
