using Application.Dtos.CreditCardTransaction;

namespace Application.Interfaces
{
    public interface ICreditCardTransactionService : IGenericService<CreditCardTransactionDto>
    {
        Task<(int TotalPayments, int TodayPayments)> GetPaymentsIndicatorsAsync();
    }
}
