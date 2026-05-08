using Application.Dtos.SavingsAccount;
using Application.Dtos.Transaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Transaction
{
    public class CustomerTransactionHistoryViewModel
    {
        // Lista de cuentas del cliente autenticado
        public List<SavingsAccountDto> UserAccounts { get; set; } = new();

        // Cuenta seleccionada por el cliente (si aplica)
        public Guid? SelectedAccountId { get; set; }

        // Número de cuenta seleccionada (para mostrar en encabezado)
        public string? SelectedAccountNumber { get; set; }

        // Historial de transacciones filtrado por cuenta
        public List<TransactionDto> Transactions { get; set; } = new();

        // Mensaje opcional si no hay transacciones
        public string? Message { get; set; }
    }
}
