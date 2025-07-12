using AutoMapper;
using Shared.DTOs;
using Shared.Entities;

namespace Server.Mappings
{
    public class MappingProfile : Profile
    {

        public MappingProfile()
        {
            CreateMap<UserCreateDto, User>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore())
                    .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            CreateMap<UserUpdateDto, User>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore())
                    .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            CreateMap<LinkCreateDto, Link>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore())
                    .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            CreateMap<LinkUpdateDto, Link>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore())
                    .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
        }
    }
}
