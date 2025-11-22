using Application.Dtos.SavingsAccount;
using Application.Dtos.Transaction;
using Application.Interfaces;
using AutoMapper;
using Domain.Common.Enums;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class SavingsAccountService : GenericService<SavingsAccount, SavingsAccountDto>, ISavingsAccountService
    {
        private readonly ISavingsAccountRepository savingsAccountRepository;
        private readonly ITransactionRepository transactionRepository;
        private readonly IMapper mapper;
        private readonly IUserAccountService _IUsserAccountService;
        public SavingsAccountService(ISavingsAccountRepository savingsAccountRepository, ITransactionRepository transactionRepository, IUserAccountService userAccountService, IMapper mapper) : base(savingsAccountRepository, mapper)
        {
            this.savingsAccountRepository = savingsAccountRepository;
            this.mapper = mapper;
            this.transactionRepository = transactionRepository;
            _IUsserAccountService = userAccountService;
        }

        public async Task<bool> AddBalanceAsync(Guid accountId, decimal amount)
        {
            var account = await savingsAccountRepository.GetById(accountId);
            if (account == null)
            {
                return false;
            }

            account.Balance += amount;
            await savingsAccountRepository.UpdateAsync(account.Id, account);
            return true;
        }

        public async Task<Guid?> GetAccountIdByAccountNumberAsync(string accountNumber)
        {
            var account = await savingsAccountRepository.GetByAccountNumberAsync(accountNumber);
            return account?.Id;
        }

        public async Task<bool> ExistsAccountNumberAsync(string accountNumber)
        {
            return await savingsAccountRepository.ExistsAccountNumberAsync(accountNumber);
        }

        public async Task<string> GenerateUniqueAccountNumberAsync()
        {
            var random = new Random();
            string accountNumber;

            do
            {
                accountNumber = random.Next(100000000, 999999999).ToString();
            }
            while (await savingsAccountRepository.ExistsAccountNumberAsync(accountNumber));

            return accountNumber;
        }


        public async Task<SavingsAccountDetailDto?> GetAccountDetailAsync(Guid accountId)
        {
            var account = await savingsAccountRepository.GetById(accountId);
            if (account == null) return null;

            var user = await _IUsserAccountService.GetUserById(account.UserId.ToString());
            if (user == null) return null;

            var transactions = await transactionRepository.GetAllByAccountIdOrderedAsync(accountId);

            var transactionDtos = transactions.Select(tx => new TransactionDto
            {
                Id = tx.Id,
                OriginAccountId = tx.OriginAccountId,
                DestinationAccountId = tx.DestinationAccountId,
                Amount = tx.Amount,
                Date = tx.Date,
                Type = tx.Type, // Deposit, Transfer, etc.

                Status = tx.Status,
                Origin = string.IsNullOrWhiteSpace(tx.Origin) ? "DEPÓSITO" : tx.Origin,
                Beneficiary = string.IsNullOrWhiteSpace(tx.Beneficiary) ? "RETIRO" : tx.Beneficiary,

                VisualType = tx.OriginAccountId == accountId ? "DÉBITO" : "CRÉDITO"
            }).ToList();

            var dto = mapper.Map<SavingsAccountDetailDto>(account);
            dto.OwnerFullName = $"{user.Name?.Trim()} {user.LastName?.Trim()}".Trim();
            dto.DocumentNumber = user.DocumentNumber;
            dto.Transactions = transactionDtos;

            return dto;
        }
        public async Task<List<SavingsAccountSummaryDto>> GetFilteredAccountsAsync(string? documentNumber, bool? isActive, bool? isPrimary, int page, int pageSize)
        {
            Guid? userId = null;

            if (!string.IsNullOrWhiteSpace(documentNumber))
            {
                var normalized = documentNumber.Replace("-", "").Trim().PadLeft(11, '0');

                var user = await _IUsserAccountService.GetAllActiveUsers();
                var match = user.FirstOrDefault(u =>
                    u.DocumentNumber.Replace("-", "").Trim().PadLeft(11, '0') == normalized);
                if (match != null && Guid.TryParse(match.Id, out var parsedId))
                {
                    userId = parsedId;
                }
            }

            var accounts = userId.HasValue
                ? await savingsAccountRepository.GetAllByUserIdOrderedAsync(userId.Value)
                : await savingsAccountRepository.GetFilteredAsync(isActive, isPrimary, page, pageSize);

            var users = await _IUsserAccountService.GetUsersByIds(accounts.Select(a => a.UserId).Distinct().ToList());

            var result = accounts.Select(account =>
            {
                var user = users.FirstOrDefault(u => u.Id == account.UserId.ToString());
                var dto = mapper.Map<SavingsAccountSummaryDto>(account);
                dto.OwnerFullName = user != null ? $"{user.Name} {user.LastName}".Trim() : "Desconocido";
                dto.DocumentNumber = user?.DocumentNumber ?? "N/D";
                return dto;
            }).ToList();

            return result;
        }

        public async Task<List<SavingsAccountDto>> GetAllByUserIdOrderedAsync(Guid userId)
        {
            var accounts = await savingsAccountRepository.GetAllByUserIdOrderedAsync(userId);
            return mapper.Map<List<SavingsAccountDto>>(accounts);
        }

        public async Task<bool> CreateSecondaryAccountAsync(CreateSavingsAccountDto dto)
        {
            var account = new SavingsAccount
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                AccountNumber = await GenerateUniqueAccountNumberAsync(),
                Balance = dto.InitialBalance,
                IsPrimary = false,
                Status = SavingsAccountStatus.Activa
            };

            var accountDto = mapper.Map<SavingsAccountDto>(account);
            await AddAsync(accountDto);

            return true;
        }

        public async Task<bool> CancelSecondaryAccountAsync(Guid accountId)
        {
            var secondary = await savingsAccountRepository.GetSecondaryByIdAsync(accountId);
            if (secondary == null) return false;

            // Evitar doble cancelación
            if (secondary.Status != SavingsAccountStatus.Activa) return false;

            //  Transferencia de balance si existe
            if (secondary.Balance > 0)
            {
                var primary = await savingsAccountRepository.GetPrimaryByUserIdAsync(secondary.UserId);
                if (primary == null) return false;

                primary.Balance += secondary.Balance;
                secondary.Balance = 0;

                await savingsAccountRepository.UpdateAsync(primary.Id, primary);
            }

            //  Cancelación lógica
            secondary.Status = SavingsAccountStatus.Cancelada;
            await savingsAccountRepository.UpdateAsync(secondary.Id, secondary);

            //  Mantener usuario activo si tiene cuenta principal
            await _IUsserAccountService.SetUserActiveStatus(secondary.UserId.ToString(), true);

            return true;
        }

        public async Task<SavingsAccountSummaryDto?> GetAccountSummaryAsync(Guid accountId)
        {
            var account = await savingsAccountRepository.GetByIdAsync(accountId);
            if (account == null) return null;

            return mapper.Map<SavingsAccountSummaryDto>(account);
        }


        public async Task<List<SavingsAccountDto>> GetActiveByUserIdAsync(Guid userId)
        {
            var all = await GetAllByUserIdOrderedAsync(userId);
            return all
                .Where(a => a.Status == SavingsAccountStatus.Activa)
                .ToList();
        }

        public async Task<SavingsAccountDto?> GetByAccountNumberAsync(string accountNumber)
        {
            var entity = await savingsAccountRepository.GetByAccountNumberAsync(accountNumber);
            return entity == null ? null : mapper.Map<SavingsAccountDto>(entity);
        }

        // Retorna todas las cuentas de ahorro registradas en el sistema.
        // Se utiliza en el Dashboard para calcular el total global.
        public async Task<List<SavingsAccountDto>> GetAllSavingsAccountsAsync()
        {
            // Consulta al repositorio para obtener todas las cuentas
            var accounts = await savingsAccountRepository.GetAllSavingsAccountsAsync();

            // Convierte las entidades SavingsAccount a DTOs
            return mapper.Map<List<SavingsAccountDto>>(accounts);
        }


        public async Task<bool> ActivatePrimaryAccountAsync(Guid userId)
        {
            var account = await savingsAccountRepository.GetPrimaryByUserIdAsync(userId);
            if (account == null)
                return false;

            account.Status = SavingsAccountStatus.Activa;
            await savingsAccountRepository.UpdateAsync(account.Id, account);
            return true;
        }

        public async Task<bool> DeactivatePrimaryAccountAsync(Guid userId)
        {
            var account = await savingsAccountRepository.GetPrimaryByUserIdAsync(userId);
            if (account == null)
                return false;

            account.Status = SavingsAccountStatus.Cancelada; // o Suspendida, según tu lógica
            await savingsAccountRepository.UpdateAsync(account.Id, account);
            return true;
        }

        public async Task<bool> AddBalanceToPrimaryAccountAsync(Guid userId, decimal amount)
        {
            if (amount <= 0)
                return false; // no aceptamos montos negativos o cero

            return await savingsAccountRepository.AddBalanceToPrimaryAccountAsync(userId, amount);
        }



    }
}
