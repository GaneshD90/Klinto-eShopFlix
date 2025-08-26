using AuthService.Application.DTO;
using AuthService.Domain.Entities;
using AutoMapper;


namespace AuthService.Application.Mappers
{
    public  class Authmapper : Profile
    {
        public Authmapper()
        {
            CreateMap<User, UserDTO>()
              .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles.Select(r => r.Name).ToArray()));

            CreateMap<SignUpDTO, User>();
        }
    }
    
}
