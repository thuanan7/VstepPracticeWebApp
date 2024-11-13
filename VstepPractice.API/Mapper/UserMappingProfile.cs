using VstepPractice.API.Models.Entities;
using AutoMapper;
using VstepPractice.API.Common.Utils;
using VstepPractice.API.Models.DTOs.Users.Requests;
using VstepPractice.API.Models.DTOs.Users.Responses;
using VstepPractice.API.Models.DTOs.Exams.Responses;
using VstepPractice.API.Models.DTOs.Sections.Responses;
using VstepPractice.API.Models.DTOs.Questions.Responses;
using VstepPractice.API.Models.DTOs.Passage.Responses;
using VstepPractice.API.Models.DTOs.SectionParts.Responses;

namespace VstepPractice.API.Mapper;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<PagedResult<User>, PagedResult<UserDto>>().ReverseMap();

        CreateMap<CreateUserDto, User>().ReverseMap();
        CreateMap<UpdateUserDto, User>().ReverseMap();

        CreateMap<Exam, ExamResponse>().ReverseMap();
        CreateMap<Section, SectionResponse>().ReverseMap();
        CreateMap<SectionPart, SectionPartResponse>().ReverseMap();
        CreateMap<Passage, PassageResponse>().ReverseMap();
        CreateMap<Question, QuestionResponse>().ReverseMap();
        CreateMap<QuestionOption, QuestionOptionResponse>().ReverseMap();
    }
}
