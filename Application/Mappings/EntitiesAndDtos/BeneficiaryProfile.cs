using Application.Dtos.Beneficiary;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mappings.EntitiesAndDtos
{
    public class BeneficiaryProfile : Profile
    {
        public BeneficiaryProfile()
        {
            // Mapeo completo para uso interno, trazabilidad o administración
            CreateMap<Beneficiary, BeneficiaryDto>();

            // Mapeo para el listado visual en la vista
            CreateMap<Beneficiary, BeneficiaryListItemDto>();

            // Mapeo inverso para crear desde el formulario
            CreateMap<CreateBeneficiaryDto, Beneficiary>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // Se genera en el servicio
                .ForMember(dest => dest.OwnerUserId, opt => opt.Ignore()) // Se asigna en el servicio
                .ForMember(dest => dest.BeneficiaryUserId, opt => opt.Ignore()) // Se obtiene desde la cuenta
                .ForMember(dest => dest.Name, opt => opt.Ignore()) // Se obtiene desde la cuenta
                .ForMember(dest => dest.LastName, opt => opt.Ignore()); // Se obtiene desde la cuenta
        }
    }

}
