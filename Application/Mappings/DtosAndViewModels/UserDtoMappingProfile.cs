using Application.Dtos.User;
using Application.ViewModels.User;
using AutoMapper;

namespace Application.Mappings.DtosAndViewModels
{
    public class UserDtoMappingProfile : Profile
    {
        public UserDtoMappingProfile() {
           
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
