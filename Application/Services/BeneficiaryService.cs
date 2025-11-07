using Application.Dtos.Beneficiary;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class BeneficiaryService : GenericService<Beneficiary, BeneficiaryDto>, IBeneficiaryService
    {
        private readonly IBeneficiaryRepository beneficiaryRepository;
        private readonly IMapper mapper;
        public BeneficiaryService(IBeneficiaryRepository beneficiaryRepository, IMapper mapper) : base(beneficiaryRepository, mapper)
        {
            this.beneficiaryRepository = beneficiaryRepository;
            this.mapper = mapper;
        }
    }
}