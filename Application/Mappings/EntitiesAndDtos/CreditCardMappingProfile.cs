using Application.Dtos.CreditCard;
using AutoMapper;
using Domain.Entities;
namespace Application.Mappings.EntitiesAndDtos
{
    public class CreditCardMappingProfile : Profile
    {
        public CreditCardMappingProfile()
        {
            CreateMap<CreditCard, CreditCardDto>().ReverseMap();

            CreateMap<CreditCard, CreateCreditCardDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.AdminUserId, opt => opt.MapFrom(src => src.AdminUserId))
                .ForMember(dest => dest.CreditLimit, opt => opt.MapFrom(src => src.CreditLimit));


            CreateMap<CreditCard, CancelCreditCardDto>()
               .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.CardLastDigits, opt => opt.MapFrom(src => src.CardNumber.Substring(src.CardNumber.Length - 4)))
               .ForMember(dest => dest.CurrentDebt, opt => opt.MapFrom(src => src.CurrentDebt));


            CreateMap<CreditCard, CreditCardDetailsDto>()
                .ForMember(dest => dest.CreditCardId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.CardNumber, opt => opt.MapFrom(src => src.CardNumber))
                .ForMember(dest => dest.ExpirationDate, opt => opt.MapFrom(src => src.ExpirationDate))
                .ForMember(dest => dest.CreditLimit, opt => opt.MapFrom(src => src.CreditLimit))
                .ForMember(dest => dest.CurrentDebt, opt => opt.MapFrom(src => src.CurrentDebt))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.HolderName, opt => opt.Ignore())
                .ForMember(dest => dest.HolderLastName, opt => opt.Ignore());

        }
    }

}