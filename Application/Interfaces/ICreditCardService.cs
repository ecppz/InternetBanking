using Application.Dtos.CreditCard;
using Application.Dtos.User;


namespace Application.Interfaces
{
    public interface ICreditCardService : IGenericService<CreditCardDto>
    {
        Task<CreditCardResponseDto> AssignCardAsync(AssignCreditCardDto dto);
        Task<bool> CancelCardAsync(CancelCreditCardDto dto);
        Task<int> ExpireCardsAsync();
        Task<List<CreditCardDto>> GetActiveCardsAsync();
        Task<List<CreditCardDto>> GetActiveCardsByUserIdAsync(Guid userId);
        Task<int> GetActiveCreditCardsCountAsync();
        Task<List<CreditCardDisplayDto>> GetAllDisplayAsync(List<UserDto> users, string? documentNumber, string? statusFilter);
        Task<decimal> GetAverageDebtAsync();
        Task<List<CreditCardDto>> GetCancelledCardsAsync();
        Task<CreditCardDetailsDto> GetCardDetailsAsync(Guid cardId);
        Task<Guid?> GetCardIdByNumberAsync(string cardNumber);
        Task<List<EligibleCustomerForCreditCardDto>> GetEligibleCustomersForCreditCard(List<UserDto> customers);
        Task<bool> UpdateCreditLimitAsync(EditCreditCardDto dto, UserDto user);
    }
}