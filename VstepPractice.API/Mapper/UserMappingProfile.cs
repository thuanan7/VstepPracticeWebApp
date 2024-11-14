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
using VstepPractice.API.Models.DTOs.StudentAttempts.Responses;

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

        CreateMap<StudentAttempt, AttemptResponse>()
            .ForMember(dest => dest.ExamTitle,
                opt => opt.MapFrom(src => src.Exam.Title))
            .ForMember(dest => dest.Answers,
                opt => opt.MapFrom(src => src.Answers));

        CreateMap<Answer, AnswerResponse>()
            .ForMember(dest => dest.QuestionText,
                opt => opt.MapFrom(src => src.Question.QuestionText))
            .ForMember(dest => dest.PassageTitle,
                opt => opt.MapFrom(src => src.Question.Passage.Title))
            .ForMember(dest => dest.PassageContent,
                opt => opt.MapFrom(src => src.Question.Passage.Content))
            .ForMember(dest => dest.IsCorrect,
                opt => opt.MapFrom(src =>
                    src.SelectedOptionId.HasValue &&
                    src.Question.Options.Any(o =>
                        o.Id == src.SelectedOptionId &&
                        o.IsCorrect)));

        // Add map for AttemptResultResponse if not already defined
        CreateMap<StudentAttempt, AttemptResultResponse>()
            .ForMember(dest => dest.ExamTitle,
                opt => opt.MapFrom(src => src.Exam.Title))
            .ForMember(dest => dest.Answers,
                opt => opt.MapFrom(src => src.Answers))
            .ForMember(dest => dest.SectionScores,
                opt => opt.Ignore()) // This is calculated manually
            .ForMember(dest => dest.TotalScore,
                opt => opt.Ignore()) // This is calculated manually
            .ForMember(dest => dest.MaximumScore,
                opt => opt.Ignore()) // This is calculated manually
            .ForMember(dest => dest.Percentage,
                opt => opt.Ignore()); // This is calculated manually
    }
}
