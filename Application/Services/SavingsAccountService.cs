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
    }
}
