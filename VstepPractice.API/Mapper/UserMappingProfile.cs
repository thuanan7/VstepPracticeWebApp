using VstepPractice.API.Models.DTOs.Users;
using VstepPractice.API.Models.Entities;
using AutoMapper;
using VstepPractice.API.Common.Utils;

namespace VstepPractice.API.Mapper;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<PagedResult<User>, PagedResult<UserDto>>().ReverseMap();

        CreateMap<CreateUserDto, User>().ReverseMap();
        CreateMap<UpdateUserDto, User>()
            .ReverseMap()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
