using Application.Dtos.CreditCardTransaction.CashAdvance;
using Application.Dtos.User;
namespace Application.Interfaces
{
    public interface ICashAdvanceService
    {
        Task<CashAdvanceDto?> ValidateAsync(CashAdvanceDto dto);
        Task<CashAdvanceDto?> RegisterCashAdvanceAsync(CashAdvanceDto dto, UserDto user);
    }
}
