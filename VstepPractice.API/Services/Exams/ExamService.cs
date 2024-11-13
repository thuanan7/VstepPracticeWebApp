using AutoMapper;
using VstepPractice.API.Common.Utils;
using VstepPractice.API.Models.DTOs.Exams.Requests;
using VstepPractice.API.Models.DTOs.Exams.Responses;
using VstepPractice.API.Models.Entities;
using VstepPractice.API.Repositories.Interfaces;

namespace VstepPractice.API.Services.Exams;

public class ExamService : IExamService
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

    public async Task<Result<PagedResult<ExamResponse>>> GetExamsByUserIdAsync(
    int userId,
    int pageIndex,
    int pageSize,
    CancellationToken cancellationToken)
    {
        // Sử dụng GetPagedAsync từ ExamRepository thay vì tự xây dựng query
        var pagedResult = await _unitOfWork.ExamRepository.GetPagedAsync(
            e => e.CreatedById == userId,
            pageIndex,
            pageSize,
            cancellationToken);

        var examResponses = _mapper.Map<List<ExamResponse>>(pagedResult.Items);
        var result = PagedResult<ExamResponse>.Create(
            examResponses,
            pagedResult.PageIndex,
            pagedResult.PageSize,
            pagedResult.TotalCount);

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
        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var user = await _unitOfWork.UserRepository.FindByIdAsync(userId, cancellationToken);
            if (user == null)
                return Result.Failure<ExamResponse>(Error.NotFound);

            var exam = new Exam
            {
                Title = request.Title,
                Description = request.Description,
                CreatedById = userId,
            };

            // Map sections with parts, passages, and questions
            foreach (var sectionRequest in request.Sections)
            {
                var section = new Section
                {
                    Type = sectionRequest.Type,
                    Title = sectionRequest.Title,
                    Instructions = sectionRequest.Instructions,
                    OrderNum = sectionRequest.OrderNum
                };

                foreach (var partRequest in sectionRequest.Parts)
                {
                    var part = new SectionPart
                    {
                        PartNumber = partRequest.PartNumber,
                        Title = partRequest.Title,
                        Instructions = partRequest.Instructions,
                        OrderNum = partRequest.OrderNum
                    };

                    foreach (var passageRequest in partRequest.Passages)
                    {
                        var passage = new Passage
                        {
                            Title = passageRequest.Title,
                            Content = passageRequest.Content,
                            AudioUrl = passageRequest.AudioUrl,
                            OrderNum = passageRequest.OrderNum
                        };

                        foreach (var questionRequest in passageRequest.Questions)
                        {
                            var question = new Question
                            {
                                QuestionText = questionRequest.QuestionText,
                                MediaUrl = questionRequest.MediaUrl,
                                Points = questionRequest.Points,
                                OrderNum = questionRequest.OrderNum,
                                Options = questionRequest.Options.Select(o => new QuestionOption
                                {
                                    OptionText = o.OptionText,
                                    IsCorrect = o.IsCorrect
                                }).ToList()
                            };

                            passage.Questions.Add(question);
                        }

                        part.Passages.Add(passage);
                    }

                    section.Parts.Add(part);
                }

                exam.Sections.Add(section);
            }

            _unitOfWork.ExamRepository.Add(exam);
            await _unitOfWork.CommitAsync(cancellationToken);

            var response = _mapper.Map<ExamResponse>(exam);
            return Result.Success(response);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
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