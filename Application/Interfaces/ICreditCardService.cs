using Application.Dtos.CreditCard;

namespace Application.Interfaces
{
    public interface ICreditCardService : IGenericService<CreditCardDto>
    {
        Task<CreditCardResponseDto> AssignCardAsync(AssignCreditCardDto dto);
        Task<List<CreditCardDto>> GetActiveCardsAsync();
        Task<List<CreditCardDto>> GetActiveCardsByUserIdAsync(Guid userId);
        Task<List<CreditCardDto>> GetCancelledCardsAsync();
        Task<List<EligibleCustomerForCreditCardDto>> GetEligibleCustomersForCreditCard();
        Task<List<CreditCardDisplayDto>> GetAllDisplayAsync(string? documentNumber, string? statusFilter);
        Task<Guid?> GetCardIdByNumberAsync(string cardNumber);
        Task<CreditCardDetailsDto> GetCardDetailsAsync(Guid cardId);
        Task<bool> UpdateCreditLimitAsync(EditCreditCardDto dto);
        Task<bool> CancelCardAsync(CancelCreditCardDto dto);
        Task<int> ExpireCardsAsync();
        Task<decimal> GetAverageDebtAsync();
    }
}
