using Application.Interfaces;
using Domain.Common.Enums;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class LoanInstallmentStatusUpdater
    {
        private readonly ILoanInstallmentRepository loanInstallmentRepository;
        private readonly IUnitOfWork unitOfWork;

        public LoanInstallmentStatusUpdater(ILoanInstallmentRepository loanInstallmentRepository, IUnitOfWork unitOfWork)
        {
            this.loanInstallmentRepository = loanInstallmentRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task UpdateLateInstallmentsAsync()
        {
            var today = DateTime.Today;
            var installments = await loanInstallmentRepository.GetAllQuery()
                .Where(i => i.Status == InstallmentStatus.Pending)
                .ToListAsync();

            foreach (var installment in installments)
            {
                if (installment.DueDate < today)
                {
                    installment.Status = InstallmentStatus.Late; 
                }
            }

            await unitOfWork.SaveAsync();
        }

    }

}
