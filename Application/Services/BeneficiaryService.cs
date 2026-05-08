using Application.Dtos.Beneficiary;
using Application.Interfaces;
using AutoMapper;
using Domain.Common.Enums;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class BeneficiaryService : GenericService<Beneficiary, BeneficiaryDto>, IBeneficiaryService
    {
        private readonly IBeneficiaryRepository beneficiaryRepository;
        private readonly ISavingsAccountRepository savingsAccountRepository;
        private readonly IMapper mapper;
        public BeneficiaryService(IBeneficiaryRepository beneficiaryRepository,ISavingsAccountRepository savingsAccountRepository ,IMapper mapper) 
            : base(beneficiaryRepository, mapper)
        {
            this.beneficiaryRepository = beneficiaryRepository;
            this.savingsAccountRepository = savingsAccountRepository;
            this.mapper = mapper;
        }

        public async Task<List<BeneficiaryDto>> GetByOwnerUserIdAsync(Guid ownerUserId)
        {
            var entities = await beneficiaryRepository.GetByOwnerUserIdAsync(ownerUserId);

            return entities.Select(b => new BeneficiaryDto
            {
                Id = b.Id,
                OwnerUserId = b.OwnerUserId,
                BeneficiaryUserId = b.BeneficiaryUserId,
                BeneficiaryAccountNumber = b.BeneficiaryAccountNumber,
                Name = b.Name,
                LastName = b.LastName
            }).ToList();
        }

        public async Task<bool> ExistsAsync(Guid ownerUserId, string beneficiaryAccountNumber)
        {
            return await beneficiaryRepository.ExistsAsync(ownerUserId, beneficiaryAccountNumber);
        }

        public async Task<(bool Success, string? ErrorMessage, BeneficiaryDto? Beneficiary)> AddAsync(Guid ownerUserId, CreateBeneficiaryDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.BeneficiaryAccountNumber))
                return (false, "Debe ingresar un número de cuenta.", null);

            if (await beneficiaryRepository.ExistsAsync(ownerUserId, dto.BeneficiaryAccountNumber))
                return (false, "Este beneficiario ya está registrado en su lista.", null);

            var account = await savingsAccountRepository.GetByAccountNumberAsync(dto.BeneficiaryAccountNumber);
            if (account is null || account.Status != SavingsAccountStatus.Activa)
                return (false, "El número ingresado no corresponde a ninguna cuenta válida.", null);

            if (account.UserId == ownerUserId)
                return (false, "No puede agregarse a sí mismo como beneficiario.", null);

            var beneficiary = new Beneficiary
            {
                Id = Guid.NewGuid(),
                OwnerUserId = ownerUserId,
                BeneficiaryAccountNumber = dto.BeneficiaryAccountNumber,
                BeneficiaryUserId = account.UserId,
            };

            await beneficiaryRepository.AddAsync(beneficiary);
            var beneficiaryDto = mapper.Map<BeneficiaryDto>(beneficiary);
            return (true, null, beneficiaryDto);
        }


        public async Task<bool> DeleteAsync(Guid beneficiaryId, Guid ownerUserId)
        {
            return await beneficiaryRepository.DeleteByIdAndOwnerAsync(beneficiaryId, ownerUserId);
        }

        public async Task<BeneficiaryDto?> GetByAccountNumberAndOwnerAsync(Guid ownerUserId, string beneficiaryAccountNumber)
        {
            var entity = await beneficiaryRepository.GetByAccountNumberAndOwnerAsync(ownerUserId, beneficiaryAccountNumber);
            if (entity == null) return null;

            return new BeneficiaryDto
            {
                Id = entity.Id,
                OwnerUserId = entity.OwnerUserId,
                BeneficiaryUserId = entity.BeneficiaryUserId,
                BeneficiaryAccountNumber = entity.BeneficiaryAccountNumber,
                Name = entity.Name,
                LastName = entity.LastName
            };
        }

    }
}