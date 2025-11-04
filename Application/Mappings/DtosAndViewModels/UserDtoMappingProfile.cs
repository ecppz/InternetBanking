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
                .ForMember(dest => dest.ProfileImageFile, opt => opt.Ignore())
                .ReverseMap()             
                .ForMember(dest => dest.ProfileImage, opt => opt.Ignore());

            CreateMap<SaveUserDto, UpdateUserViewModel>()
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.ConfirmPassword, opt => opt.Ignore())
                .ForMember(dest => dest.ProfileImageFile, opt => opt.Ignore())
                .ReverseMap()      
                .ForMember(dest => dest.ProfileImage, opt => opt.Ignore());

            CreateMap<SaveUserDto, RegisterUserViewModel>()                
                .ForMember(dest => dest.ProfileImageFile, opt => opt.Ignore())
                .ReverseMap()                               
                .ForMember(dest => dest.ProfileImage, opt => opt.Ignore());

            CreateMap<LoginDto, LoginViewModel>()
                .ReverseMap();
        }
    }
}
