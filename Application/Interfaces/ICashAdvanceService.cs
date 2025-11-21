using Application.Dtos.CreditCardTransaction.CashAdvance;
namespace Application.Interfaces
{
    public interface ICashAdvanceService
    {
        Task<CashAdvanceDto?> ValidateAsync(CashAdvanceDto dto);
        Task<CashAdvanceDto?> RegisterCashAdvanceAsync(CashAdvanceDto dto);
    }
}
