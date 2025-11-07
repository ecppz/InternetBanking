using Application.Dtos.LoanInstallment;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class LoanInstallmentService : GenericService<LoanInstallment, LoanInstallmentDto>, ILoanInstallmentService
    {

        private readonly ILoanInstallmentRepository loanInstallmentRepository;
        private readonly IMapper mapper;
        public LoanInstallmentService(ILoanInstallmentRepository loanInstallmentRepository, IMapper mapper) : base(loanInstallmentRepository, mapper)
        {
            this.loanInstallmentRepository = loanInstallmentRepository;
            this.mapper = mapper;
        }

    }
}
