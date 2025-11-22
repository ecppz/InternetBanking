using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.AdminDashboard
{
    // ViewModel principal para el Dashboard del Administrador.
    // Contiene todos los indicadores que se mostrarán en la vista.
    public class AdminDashboardViewModel
    {
        // Indicadores de Transacciones
        public int TotalTransactions { get; set; }              // Total de transacciones registradas en el sistema
        public int TodayTransactions { get; set; }              // Transacciones realizadas en la fecha actual

        // Indicadores de Pagos
        public int TotalPayments { get; set; }                  // Total de pagos procesados en el historial
        public int TodayPayments { get; set; }                  // Pagos procesados en la fecha actual

        // Indicadores de Clientes
        public int ActiveClients { get; set; }                  // Número de clientes activos
        public int InactiveClients { get; set; }                // Número de clientes inactivos

        // Indicadores de Cuentas de Ahorro
        public int TotalSavingsAccounts { get; set; }           // Total de cuentas de ahorro abiertas

        // Placeholders para indicadores pendientes (Préstamos, Tarjetas, Deuda promedio)
        public int ActiveLoans { get; set; }                    // Cantidad de préstamos vigentes (placeholder)
        public int ActiveCreditCards { get; set; }              // Número de tarjetas de crédito activas (placeholder)
        public string AverageDebtPerClient { get; set; }        // Monto promedio de deuda por cliente (placeholder)
    }
}
