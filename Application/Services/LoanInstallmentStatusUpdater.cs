using AutoMapper;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class LoanInstallmentStatusUpdater
    {
        private readonly ILoanInstallmentRepository loanInstallmentRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public LoanInstallmentStatusUpdater(ILoanInstallmentRepository loanInstallmentRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.loanInstallmentRepository = loanInstallmentRepository;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task UpdateLateInstallmentsAsync()
        {
            var today = DateTime.Today;
            var installments = await loanInstallmentRepository.GetAllQuery()
                .Where(i => !i.IsPaid)
                .ToListAsync();

            foreach (var installment in installments)
            {
                installment.IsLate = installment.DueDate < today;
            }

            await unitOfWork.SaveAsync();
        }
    }

}
