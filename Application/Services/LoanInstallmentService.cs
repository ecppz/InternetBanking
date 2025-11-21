using Application.Dtos.LoanInstallment;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class LoanInstallmentService : GenericService<LoanInstallment, LoanInstallmentDto>, ILoanInstallmentService
    {

        private readonly ILoanInstallmentRepository loanInstallmentRepository;
        private readonly ILoanRepository loanRepository;
        private readonly IMapper mapper;
        public LoanInstallmentService(ILoanInstallmentRepository loanInstallmentRepository, ILoanRepository loanRepository, IMapper mapper) : base(loanInstallmentRepository, mapper)
        {
            this.loanInstallmentRepository = loanInstallmentRepository;
            this.loanRepository = loanRepository;
            this.mapper = mapper;
        }

        public async Task<string> RecalculateInstallmentsAsync(Guid loanId, decimal newAnnualRate)
        {
            var loan = await loanRepository.GetById(loanId);
            if (loan == null)
            {
                return "Préstamo no encontrado";
            }

            var installments = await loanInstallmentRepository.GetByLoanIdAsync(loanId);
            if (installments == null || installments.Count == 0)
            {
                return "No hay cuotas para recalcular";
            }

            double r = (double)(newAnnualRate / 12 / 100);
            double n = loan.TermMonths;
            double P = (double)loan.Amount;

            double cuotaRaw = P * (r * Math.Pow(1 + r, n)) / (Math.Pow(1 + r, n) - 1);
            decimal cuota = Math.Round((decimal)cuotaRaw, 2);

            var now = DateTime.Now;

            foreach (var i in installments)
            {
                if (i.DueDate > now && !i.IsPaid)
                {
                    i.Amount = cuota;
                }
            }

            await loanInstallmentRepository.UpdateRangeAsync(installments);
            return "Cuotas recalculadas correctamente";
        }

        public async Task<LoanInstallmentDto?> GetNextPendingInstallmentAsync(Guid loanId)
        {
            var next = await loanInstallmentRepository
                .GetAllQuery()
                .Where(i => i.LoanId == loanId && !i.IsPaid)
                .OrderBy(i => i.DueDate)
                .FirstOrDefaultAsync();

            if (next == null)
            {
                return null;
            }

            return mapper.Map<LoanInstallmentDto>(next);
        }

        public async Task<LoanInstallmentDto?> MarkInstallmentAsPaidAsync(Guid loanId)
        {
            var installment = await loanInstallmentRepository
                .GetAllQuery()
                .Where(i => i.LoanId == loanId && !i.IsPaid)
                .OrderBy(i => i.DueDate)
                .FirstOrDefaultAsync();

            if (installment == null)
            {
                return null;    
            }

            installment.IsPaid = true;
            await loanInstallmentRepository.UpdateAsync(installment.Id, installment);
            return mapper.Map<LoanInstallmentDto>(installment); 
        }

    }
}
