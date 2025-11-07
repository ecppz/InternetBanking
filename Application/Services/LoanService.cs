using Application.Dtos.Loan;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class LoanService : GenericService<Loan, LoanDto>, ILoanService
    {
        private readonly ILoanRepository loanRepository;
        private readonly IMapper mapper;
        public LoanService(ILoanRepository loanRepository, IMapper mapper) : base(loanRepository, mapper)
        {
            this.loanRepository = loanRepository;
            this.mapper = mapper;
        }
    }
}
