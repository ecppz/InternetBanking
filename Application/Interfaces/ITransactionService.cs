using Application.Dtos.Transaction;
using Application.ViewModels.Transaction;
using Application.ViewModels.TransactionBeneficiaryTransfer;

namespace Application.Interfaces
{
    public interface ITransactionService : IGenericService<TransactionDto>
    {
        //Para cuenta de ahorro:

        // Retorna todas las transacciones donde la cuenta fue origen
        Task<List<TransactionDto>> GetByOriginAccountIdAsync(Guid accountId);

        // Retorna todas las transacciones donde la cuenta fue destino
        Task<List<TransactionDto>> GetByDestinationAccountIdAsync(Guid accountId);

        // Retorna todas las transacciones donde la cuenta fue origen o destino
        Task<List<TransactionDto>> GetAllByAccountIdAsync(Guid accountId);

        // Retorna todas las transacciones donde la cuenta fue origen o destino, ordenadas por fecha descendente
        Task<List<TransactionDto>> GetAllByAccountIdOrderedAsync(Guid accountId);

        // Retorna el historial completo de transacciones de un cliente (todas sus cuentas)
        Task<List<TransactionDto>> GetAllByUserIdAsync(Guid userId);

        // Retorna el historial ordenado de transacciones de un cliente
        Task<List<TransactionDto>> GetAllByUserIdOrderedAsync(Guid userId);

        //Para cajero:

        // Valida si una cuenta existe y está activa
        Task<bool> IsAccountValidAsync(string accountNumber);

        // Obtiene el nombre completo del titular de una cuenta
        Task<string?> GetAccountOwnerFullNameAsync(string accountNumber);

        // Verifica si una cuenta tiene fondos suficientes
        Task<bool> HasSufficientFundsAsync(string accountNumber, decimal amount);

        // Orquesta la transferencia entre cuentas de terceros (ejecuta y registra)
        Task<bool> ExecuteThirdPartyTransferAsync(string originAccountNumber, string destinationAccountNumber, decimal amount);

        // Genera el modelo de confirmación visual para la transferencia
        Task<ConfirmThirdPartyTransferViewModel?> PrepareTransferConfirmationAsync(string originAccountNumber, string destinationAccountNumber, decimal amount);

        Task<bool> IsOriginAccountValidAsync(string accountNumber);

        Task<bool> IsDestinationAccountValidAsync(string accountNumber);

        Task<string?> GetAccountStatusAsync(string accountNumber);

        Task RegisterRejectedTransactionAsync(string originAccountNumber, string destinationAccountNumber, decimal amount, string reason);

        //Para cajero apartado de deposito y retiro

        //Deposito
        Task<DepositConfirmationDto?> ValidateDepositAsync(DepositRequestDto request);
        Task<bool> ExecuteDepositAsync(DepositRequestDto request);

        //Retiro

        Task<WithdrawalConfirmationDto?> ValidateWithdrawalAsync(WithdrawalRequestDto request);
        Task<bool> ExecuteWithdrawalAsync(WithdrawalRequestDto request);

        //Trasaccion para beneficiarios


        Task<ConfirmBeneficiaryTransferViewModel> PrepareBeneficiaryTransferConfirmationAsync(
    string originAccountNumber,
    string beneficiaryAccountNumber,
    decimal amount,
    Guid ownerUserId
);

        Task<bool> ExecuteBeneficiaryTransferAsync(ExecuteBeneficiaryTransferDto model);

        //admin:

        // Retorna todas las transacciones registradas en el sistema.
        // Se utilizará en el Dashboard para calcular indicadores globales.
        Task<List<TransactionDto>> GetAllTransactionsAsync();

        // Retorna todas las transacciones de tipo "Pago" registradas en el sistema.
        // Se utilizará en el Dashboard para calcular la cantidad de pagos procesados.
        Task<List<TransactionDto>> GetAllPaymentsAsync();

        //para cajero:

        Task<int> GetDepositsCountByCashierAndDateAsync(Guid userId, DateTime date);

        Task<int> GetWithdrawalsCountByCashierAndDateAsync(Guid userId, DateTime date);


    }
}
