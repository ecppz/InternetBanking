using Application.Dtos.Transfer;
using Application.Interfaces;
using AutoMapper;
using Domain.Common.Enums;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class InternalTransferService : GenericService<SavingsAccount, InternalTransferRequestDto>, IInternalTransferService
    {
        private readonly ISavingsAccountRepository savingsAccountRepository;
        private readonly ITransactionRepository transactionRepository;
        private readonly IMapper mapper;

        public InternalTransferService(
            ISavingsAccountRepository savingsAccountRepository,
            ITransactionRepository transactionRepository,
            IUserAccountServiceForWebApp userAccountService,
            IMapper mapper
        ) : base(savingsAccountRepository, mapper)
        {
            this.savingsAccountRepository = savingsAccountRepository;
            this.transactionRepository = transactionRepository;
            this.mapper = mapper;
        }


        public async Task<InternalTransferResultDto> TransferAsync(Guid userId, InternalTransferRequestDto dto)
        {
            if (dto.OriginAccountId == dto.DestinationAccountId)
            {
                return new InternalTransferResultDto
                {
                    Success = false,
                    Message = "La cuenta de origen y destino no pueden ser la misma."
                };
            }

            var originAccount = await savingsAccountRepository.GetActiveByIdAndUserAsync(dto.OriginAccountId, userId);
            var destinationAccount = await savingsAccountRepository.GetActiveByIdAndUserAsync(dto.DestinationAccountId, userId);

            if (originAccount is null || destinationAccount is null)
            {
                return new InternalTransferResultDto
                {
                    Success = false,
                    Message = "Una o ambas cuentas no existen, no están activas o no le pertenecen al usuario."
                };
            }

            if (originAccount.Balance < dto.Amount)
            {
                return new InternalTransferResultDto
                {
                    Success = false,
                    Message = "Fondos insuficientes en la cuenta de origen."
                };
            }

            // Actualizar balances
            originAccount.Balance -= dto.Amount;
            destinationAccount.Balance += dto.Amount;

            await savingsAccountRepository.UpdateAsync(originAccount.Id, originAccount);
            await savingsAccountRepository.UpdateAsync(destinationAccount.Id, destinationAccount);

            // Registrar una sola transacción
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                OriginAccountId = originAccount.Id,
                DestinationAccountId = destinationAccount.Id,
                Amount = dto.Amount,
                Date = DateTime.UtcNow,
                Type = TransactionType.Transfer,
                Status = TransactionStatus.Approved,
                Origin = originAccount.AccountNumber,
                Beneficiary = destinationAccount.AccountNumber
            };

            var success = await transactionRepository.RegisterTransactionAsync(transaction);

            return new InternalTransferResultDto
            {
                Success = success,
                Message = success
                    ? "Transferencia realizada exitosamente."
                    : "Ocurrió un error al procesar la transferencia. Intente nuevamente."
            };
        }
    }

}
