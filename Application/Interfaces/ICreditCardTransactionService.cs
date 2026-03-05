using Application.Dtos.CreditCardTransaction;
using Application.Dtos.User;

namespace Application.Interfaces
{
    public interface ICreditCardTransactionService : IGenericService<CreditCardTransactionDto>
    {
        Task<CreditCardTransactionDto?> RegisterPaymentAsync(CreditCardTransactionDto dto, Guid performedbyUserId);
    }
}
