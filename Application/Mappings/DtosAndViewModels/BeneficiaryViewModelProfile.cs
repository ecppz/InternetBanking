using Application.Dtos.Beneficiary;
using Application.ViewModels.Beneficiary;
using AutoMapper;

namespace Application.Mappings.DtosAndViewModels
{
    public class BeneficiaryViewModelProfile : Profile
    {
        public BeneficiaryViewModelProfile()
        {
            // Mapeo DTO completo → ViewModel para listado y trazabilidad
            CreateMap<BeneficiaryDto, BeneficiaryViewModel>();

            // Mapeo para el listado visual (más liviano)
            CreateMap<BeneficiaryDto, BeneficiaryListViewModel>()
                .ForMember(dest => dest.Beneficiaries, opt => opt.Ignore()) // Se llena manualmente si se usa
                .ForMember(dest => dest.NewBeneficiary, opt => opt.Ignore()); // Se llena desde el modal

            // Mapeo para el formulario de creación
            CreateMap<CreateBeneficiaryDto, CreateBeneficiaryViewModel>().ReverseMap();
        }
    }

}
