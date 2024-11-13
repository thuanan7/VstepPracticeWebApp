using Microsoft.EntityFrameworkCore;
using VstepPractice.API.Common.Utils;
using VstepPractice.API.Models.DTOs.Exams.Responses;
using VstepPractice.API.Models.DTOs.Questions.Requests;
using VstepPractice.API.Models.DTOs.Questions.Responses;
using VstepPractice.API.Models.DTOs.Sections.Requests;
using VstepPractice.API.Models.Entities;

namespace VstepPractice.API.Services.Exams;

public partial class ExamService : IExamService
{
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

    public async Task<Result<ExamResponse>> UpdateExamSectionsAsync(
        int id,
        int userId,
        List<CreateSectionRequest> request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var exam = await _unitOfWork.ExamRepository.FindByIdAsync(id, cancellationToken);
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

            // Remove all existing sections
            var existingSections = exam.Sections.ToList();
            foreach (var section in existingSections)
            {
                var existingQuestions = section.Questions.ToList();
                foreach (var question in existingQuestions)
                {
                    var existingOptions = question.Options.ToList();
                    question.Options.Clear();
                    foreach (var option in existingOptions)
                    {
                        _unitOfWork.QuestionOptions.Remove(option);
                    }
                }
                section.Questions.Clear();
                exam.Sections.Remove(section);
            }

            // Add new sections
            foreach (var sectionRequest in request)
            {
                var section = new Section
                {
                    ExamId = exam.Id,
                    Title = sectionRequest.Title,
                    Instructions = sectionRequest.Instructions,
                    Type = sectionRequest.Type,
                    OrderNum = sectionRequest.OrderNum,
                    Questions = sectionRequest.Questions.Select(q => new Question
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
                };
                exam.Sections.Add(section);
            }

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

    public async Task<Result<QuestionResponse>> AddQuestionAsync(
        int examId,
        int sectionId,
        int userId,
        CreateQuestionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var exam = await _unitOfWork.ExamRepository.FindByIdAsync(examId, cancellationToken);
            if (exam == null)
            {
                return Result.Failure<QuestionResponse>(Error.NotFound);
            }

            if (exam.CreatedById != userId)
            {
                return Result.Failure<QuestionResponse>(new Error(
                    "Exam.UpdateUnauthorized",
                    "You are not authorized to update this exam."));
            }

            var section = exam.Sections.FirstOrDefault(s => s.Id == sectionId);
            if (section == null)
            {
                return Result.Failure<QuestionResponse>(new Error(
                    "Section.NotFound",
                    "Section not found in the specified exam."));
            }

            var question = new Question
            {
                SectionId = sectionId,
                QuestionText = request.QuestionText,
                MediaUrl = request.MediaUrl,
                Points = request.Points,
                Options = request.Options.Select(o => new QuestionOption
                {
                    OptionText = o.OptionText,
                    IsCorrect = o.IsCorrect
                }).ToList()
            };

            section.Questions.Add(question);
            await _unitOfWork.CommitAsync(cancellationToken);

            var response = _mapper.Map<QuestionResponse>(question);
            return Result.Success(response);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result> DeleteQuestionAsync(
        int examId,
        int sectionId,
        int questionId,
        int userId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var exam = await _unitOfWork.ExamRepository.FindByIdAsync(examId, cancellationToken);
            if (exam == null)
            {
                return Result.Failure(Error.NotFound);
            }

            if (exam.CreatedById != userId)
            {
                return Result.Failure(new Error(
                    "Exam.DeleteUnauthorized",
                    "You are not authorized to delete questions from this exam."));
            }

            var section = exam.Sections.FirstOrDefault(s => s.Id == sectionId);
            if (section == null)
            {
                return Result.Failure(new Error(
                    "Section.NotFound",
                    "Section not found in the specified exam."));
            }

            var question = section.Questions.FirstOrDefault(q => q.Id == questionId);
            if (question == null)
            {
                return Result.Failure(new Error(
                    "Question.NotFound",
                    "Question not found in the specified section."));
            }

            // Remove all options first
            var options = question.Options.ToList();
            foreach (var option in options)
            {
                question.Options.Remove(option);
            }

            // Then remove the question
            section.Questions.Remove(question);

            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    // Private helper methods
    private async Task<bool> IsExamExistAsync(
        int examId,
        CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExamRepository.FindAll(e => e.Id == examId)
            .AnyAsync(cancellationToken);
    }

    private async Task<bool> HasUserAccessToExamAsync(
        int examId,
        int userId,
        CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExamRepository.FindAll(
            e => e.Id == examId && e.CreatedById == userId)
            .AnyAsync(cancellationToken);
    }
}
