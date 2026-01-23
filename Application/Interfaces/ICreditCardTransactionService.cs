using Application.Dtos.CreditCardTransaction;

namespace Application.Interfaces
{
    public interface ICreditCardTransactionService : IGenericService<CreditCardTransactionDto>
    {
        Task<CreditCardTransactionDto?> RegisterPaymentAsync(CreditCardTransactionDto dto, Guid userId);
    }
}
