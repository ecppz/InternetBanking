using Application.Dtos.SavingsAccount;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class SavingsAccountService : GenericService<SavingsAccount, SavingsAccountDto>, ISavingsAccountService
    {
        private readonly ISavingsAccountRepository savingsAccountRepository;
        private readonly IMapper mapper;
        public SavingsAccountService(ISavingsAccountRepository savingsAccountRepository, IMapper mapper) : base(savingsAccountRepository, mapper)
        {
            this.savingsAccountRepository = savingsAccountRepository;
            this.mapper = mapper;
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
    }
}
