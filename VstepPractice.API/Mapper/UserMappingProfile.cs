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
using VstepPractice.API.Common.Enums;

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

        // Update StudentAttempt to AttemptResultResponse mapping
        CreateMap<StudentAttempt, AttemptResultResponse>()
            .ForMember(dest => dest.Id,
                opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ExamTitle,
                opt => opt.MapFrom(src => src.Exam.Title))
            .ForMember(dest => dest.StartTime,
                opt => opt.MapFrom(src => src.StartTime))
            .ForMember(dest => dest.EndTime,
                opt => opt.MapFrom(src => src.EndTime!))
            .ForMember(dest => dest.Answers,
                opt => opt.MapFrom(src => src.Answers))
            .ForMember(dest => dest.SectionScores,
                opt => opt.Ignore()) // Calculated in service
            .ForMember(dest => dest.FinalScore,
                opt => opt.Ignore()); // Calculated in service

        // Update Answer to AnswerResponse mapping
        CreateMap<Answer, AnswerResponse>()
            .ForMember(dest => dest.QuestionText,
                opt => opt.MapFrom(src => src.Question.QuestionText))
            .ForMember(dest => dest.SectionTitle,
                opt => opt.MapFrom(src => src.Question.Section.Title)) // Map section title
            .ForMember(dest => dest.PartTitle,
                opt => opt.MapFrom(src => src.Question.Part.Title))   // Map part title
            .ForMember(dest => dest.PassageTitle,
                opt => opt.MapFrom(src => src.Question.Passage.Title))
            .ForMember(dest => dest.PassageContent,
                opt => opt.MapFrom(src => src.Question.Passage.Content))
            .ForMember(dest => dest.Score,
                opt => opt.MapFrom((src, dest, _, context) =>
                {
                    if (src.Question.Section.Type == SectionType.Writing)
                    {
                        if (context.TryGetItems(out var items) &&
                            items.TryGetValue("WritingAssessment", out var assessmentObj) &&
                            assessmentObj is WritingAssessment assessment)
                        {
                            return assessment.TotalScore;
                        }
                        return null;
                    }
                    return src.Score;
                }))
            .ForMember(dest => dest.WritingScore,
                opt => opt.MapFrom((src, dest, _, context) =>
                {
                    if (src.Question.Section.Type != SectionType.Writing)
                        return null;

                    if (context.TryGetItems(out var items) &&
                        items.TryGetValue("WritingAssessment", out var assessmentObj) &&
                        assessmentObj is WritingAssessment assessment)
                    {
                        return new WritingScoreDetails
                        {
                            TaskAchievement = assessment.TaskAchievement,
                            CoherenceCohesion = assessment.CoherenceCohesion,
                            LexicalResource = assessment.LexicalResource,
                            GrammarAccuracy = assessment.GrammarAccuracy
                        };
                    }
                    return null;
                }))
            .ForMember(dest => dest.IsCorrect,
                opt => opt.MapFrom(src =>
                    src.SelectedOptionId.HasValue &&
                    src.Question.Options.Any(o =>
                        o.Id == src.SelectedOptionId &&
                        o.IsCorrect)));

        // Update mapping for any nested objects if needed
        CreateMap<Section, SectionResponse>()
            .ForMember(dest => dest.Title,
                opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Type,
                opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.Parts,
                opt => opt.MapFrom(src => src.Parts))
            .ForMember(dest => dest.OrderNum,
                opt => opt.MapFrom(src => src.OrderNum));
    }
}
