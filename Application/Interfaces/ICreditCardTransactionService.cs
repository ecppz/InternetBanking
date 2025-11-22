using Application.Dtos.CreditCardTransaction;

namespace Application.Interfaces
{
    public interface ICreditCardTransactionService : IGenericService<CreditCardTransactionDto>
    {
        Task<CreditCardTransactionDto?> RegisterPaymentAsync(CreditCardTransactionDto dto);
        Task<(int TotalPayments, int TodayPayments)> GetPaymentsIndicatorsAsync();
    }
}
