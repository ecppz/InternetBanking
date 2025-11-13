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
        private readonly IUserAccountService userAccountService;
        private readonly IMapper mapper;
        public BeneficiaryService(IBeneficiaryRepository beneficiaryRepository,IUserAccountService userAccountService,ISavingsAccountRepository savingsAccountRepository ,IMapper mapper) : base(beneficiaryRepository, mapper)
        {
            this.beneficiaryRepository = beneficiaryRepository;
            this.savingsAccountRepository = savingsAccountRepository;
            this.userAccountService = userAccountService;
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

        public async Task<(bool Success, string? ErrorMessage)> AddAsync(Guid ownerUserId, CreateBeneficiaryDto dto)
        {
            // Validación: campo obligatorio
            if (string.IsNullOrWhiteSpace(dto.BeneficiaryAccountNumber))
                return (false, "Debe ingresar un número de cuenta.");

            // Validación: cuenta ya registrada como beneficiario
            if (await beneficiaryRepository.ExistsAsync(ownerUserId, dto.BeneficiaryAccountNumber))
                return (false, "Este beneficiario ya está registrado en su lista.");

            // Validación: cuenta existe y está activa
            var account = await savingsAccountRepository.GetByAccountNumberAsync(dto.BeneficiaryAccountNumber);
            if (account is null || account.Status != SavingsAccountStatus.Activa)
                return (false, "El número ingresado no corresponde a ninguna cuenta válida.");

            // Validación: no puede agregarse a sí mismo
            if (account.UserId == ownerUserId)
                return (false, "No puede agregarse a sí mismo como beneficiario.");

            // Obtener datos del titular de la cuenta beneficiaria
            var user = await userAccountService.GetUserById(account.UserId.ToString());
            if (user is null)
                return (false, "No se pudo obtener la información del titular de la cuenta.");

            // Crear entidad
            var beneficiary = new Beneficiary
            {
                Id = Guid.NewGuid(),
                OwnerUserId = ownerUserId,
                BeneficiaryAccountNumber = dto.BeneficiaryAccountNumber,
                BeneficiaryUserId = account.UserId,
                Name = user.Name,
                LastName = user.LastName
            };

            await beneficiaryRepository.AddAsync(beneficiary);
            return (true, null);
        }
        public async Task<bool> DeleteAsync(Guid beneficiaryId, Guid ownerUserId)
        {
            return await beneficiaryRepository.DeleteByIdAndOwnerAsync(beneficiaryId, ownerUserId);
        }



    }
}