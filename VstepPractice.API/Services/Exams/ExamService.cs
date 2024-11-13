using AutoMapper;
using VstepPractice.API.Common.Utils;
using VstepPractice.API.Models.DTOs.Exams.Requests;
using VstepPractice.API.Models.DTOs.Exams.Responses;
using VstepPractice.API.Models.Entities;
using VstepPractice.API.Repositories.Interfaces;

namespace VstepPractice.API.Services.Exams;

public partial class ExamService : IExamService
{
    private readonly IExamRepository _examRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;

    public ExamService(
        IExamRepository examRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IUserRepository userRepository)
    {
        _examRepository = examRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _userRepository = userRepository;
    }

    public async Task<Result<PagedResult<ExamResponse>>> GetExamsAsync(
        int pageIndex = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var pagedResult = await _examRepository.GetPagedAsync(
            null, pageIndex, pageSize, cancellationToken);

        var examResponses = _mapper.Map<List<ExamResponse>>(pagedResult.Items);
        var result = PagedResult<ExamResponse>.Create(
            examResponses, pagedResult.PageIndex, pagedResult.PageSize, pagedResult.TotalCount);

        return Result.Success(result);
    }

    public async Task<Result<ExamResponse>> GetExamByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var exam = await _examRepository.FindByIdAsync(id, cancellationToken);
        if (exam == null)
        {
            return Result.Failure<ExamResponse>(Error.NotFound);
        }

        var response = _mapper.Map<ExamResponse>(exam);
        return Result.Success(response);
    }

    public async Task<Result<ExamResponse>> CreateExamAsync(
        int userId,
        CreateExamRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.FindByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return Result.Failure<ExamResponse>(Error.NotFound);
        }

        var exam = new Exam
        {
            Title = request.Title,
            Description = request.Description,
            CreatedById = userId,
            Sections = request.Sections.Select(s => new Section
            {
                Title = s.Title,
                Instructions = s.Instructions,
                Type = s.Type,
                OrderNum = s.OrderNum,
                Questions = s.Questions.Select(q => new Question
                {
                    QuestionText = q.QuestionText,
                    MediaUrl = q.MediaUrl,
                    Points = q.Points,
                    Options = q.Options.Select(o => new QuestionOption
                    {
                        OptionText = o.OptionText,
                        IsCorrect = o.IsCorrect
                    }).ToList()
                }).ToList()
            }).ToList()
        };

        _examRepository.Add(exam);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = _mapper.Map<ExamResponse>(exam);
        return Result.Success(response);
    }

    public async Task<Result<ExamResponse>> UpdateExamAsync(
        int id,
        int userId,
        UpdateExamRequest request,
        CancellationToken cancellationToken = default)
    {
        var exam = await _examRepository.FindByIdAsync(id, cancellationToken);
        if (exam == null)
        {
            return Result.Failure<ExamResponse>(Error.NotFound);
        }

        if (exam.CreatedById != userId)
        {
            return Result.Failure<ExamResponse>(new Error(
                "Exam.UpdateUnauthorized",
                "You are not authorized to update this exam."));
        }

        exam.Title = request.Title;
        exam.Description = request.Description;

        _examRepository.Update(exam);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = _mapper.Map<ExamResponse>(exam);
        return Result.Success(response);
    }

    public async Task<Result> DeleteExamAsync(
        int id,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var exam = await _examRepository.FindByIdAsync(id, cancellationToken);
        if (exam == null)
        {
            return Result.Failure(Error.NotFound);
        }

        if (exam.CreatedById != userId)
        {
            return Result.Failure(new Error(
                "Exam.DeleteUnauthorized",
                "You are not authorized to delete this exam."));
        }

        _examRepository.Remove(exam);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}