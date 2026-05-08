namespace Application.ViewModels.Cashier
{
    public class CashierHomeViewModel
    {
        // Indicadores del cajero logueado en el día
        public int TodayTransactions { get; set; }
        public int TodayPayments { get; set; }
        public int TodayDeposits { get; set; }
        public int TodayWithdrawals { get; set; }
    }

}
