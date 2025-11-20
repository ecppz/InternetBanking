namespace Application.ViewModels.ExpressTransaction
{
    public class ConfirmThirdPartyTransferCustomerViewModel
    {
        public string OriginAccountNumber { get; set; } = string.Empty;
        public string DestinationAccountNumber { get; set; } = string.Empty;
        public string DestinationFullName { get; set; } = "Desconocido";
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
