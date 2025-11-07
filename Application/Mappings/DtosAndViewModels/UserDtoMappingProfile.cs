using Application.Dtos.User;
using Application.ViewModels.User;
using AutoMapper;

namespace Application.Mappings.DtosAndViewModels
{
    public class UserDtoMappingProfile : Profile
    {
        public UserDtoMappingProfile() {





            CreateMap<UserDto, UserViewModel>()
     .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
     .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
     .ReverseMap();

            // Para poblar la vista de edición desde el DTO
            // Para poblar la vista de edición desde el DTO
            CreateMap<UserDto, UpdateUserViewModel>()
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.ConfirmPassword, opt => opt.Ignore());

            // Para enviar los datos editados al backend
            CreateMap<UpdateUserViewModel, SaveUserDto>()
                .ForMember(dest => dest.Password, opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.Password)))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role)) // se puede editar
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email)) // se puede editar
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName)); // se puede editar

            CreateMap<SaveUserDto, SaveUserViewModel>().ReverseMap();

            CreateMap<SaveUserViewModel, SaveUserDto>()
          .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

           
            CreateMap<UserDto, UserViewModel>()             
                .ReverseMap();

            CreateMap<UserDto, UpdateUserViewModel>()
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.ConfirmPassword, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<SaveUserDto, UpdateUserViewModel>()
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.ConfirmPassword, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<SaveUserDto, SaveUserViewModel>().ReverseMap();                         

            CreateMap<LoginDto, LoginViewModel>().ReverseMap();
        }
    }
}
