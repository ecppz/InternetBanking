using Application.Dtos.CreditCardTransaction;
using Application.Interfaces;
using AutoMapper;
using Domain.Common.Enums;
using Domain.Entities;
using Domain.Interfaces;


namespace Application.Services
{
    public class CreditCardTransactionService : GenericService<CreditCardTransaction, CreditCardTransactionDto>, ICreditCardTransactionService
    {
        private readonly ICreditCardTransactionRepository creditCardTransactionRepository;
        private readonly IMapper mapper;
        public CreditCardTransactionService(ICreditCardTransactionRepository creditCardTransactionRepository, IMapper mapper) : base(creditCardTransactionRepository, mapper)
        {
            this.creditCardTransactionRepository = creditCardTransactionRepository;
            this.mapper = mapper;
        }



        public async Task<(int TotalPayments, int TodayPayments)> GetPaymentsIndicatorsAsync()
        {
            var allPayments = await creditCardTransactionRepository.GetAllTransactionsAsync();

            var approvedPayments = allPayments.Where(t => (int)t.Status == (int)TransactionStatus.Approved);

            var totalPayments = approvedPayments.Count();
            var todayPayments = approvedPayments.Count(t => t.Date.Date == DateTime.Today);

            return (totalPayments, todayPayments);
        }


    }
}
