using AutoMapper;
using VstepPractice.API.Common.Enums;
using VstepPractice.API.Common.Utils;
using VstepPractice.API.Models.DTOs.StudentAttempts.Requests;
using VstepPractice.API.Models.DTOs.StudentAttempts.Responses;
using VstepPractice.API.Models.Entities;
using VstepPractice.API.Repositories.Interfaces;
using VstepPractice.API.Services.AI;
using VstepPractice.API.Services.BackgroundServices;

namespace VstepPractice.API.Services.StudentAttempts;

public class StudentAttemptService : IStudentAttemptService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEssayScoringQueue _scoringQueue;

    public StudentAttemptService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IEssayScoringQueue scoringQueue)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _scoringQueue = scoringQueue;
    }

    public async Task<Result<AttemptResponse>> StartAttemptAsync(
        int userId,
        StartAttemptRequest request,
        CancellationToken cancellationToken = default)
    {
        var exam = await _unitOfWork.ExamRepository.FindByIdAsync(
            request.ExamId, cancellationToken);

        if (exam == null)
            return Result.Failure<AttemptResponse>(Error.NotFound);

        // Check if user has any in-progress attempts
        var hasInProgressAttempt = await _unitOfWork.StudentAttemptRepository
            .HasInProgressAttempt(userId, request.ExamId, cancellationToken);

        if (hasInProgressAttempt)
            return Result.Failure<AttemptResponse>(
                new Error("Attempt.InProgress", "You have an in-progress attempt for this exam."));

        var attempt = new StudentAttempt
        {
            UserId = userId,
            ExamId = request.ExamId,
            StartTime = DateTime.UtcNow,
            Status = AttemptStatus.InProgress
        };

        _unitOfWork.StudentAttemptRepository.Add(attempt);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = _mapper.Map<AttemptResponse>(attempt);
        return Result.Success(response);
    }

    public async Task<Result<AnswerResponse>> SubmitAnswerAsync(
        int userId,
        int attemptId,
        SubmitAnswerRequest request,
        CancellationToken cancellationToken = default)
    {
        var attempt = await _unitOfWork.StudentAttemptRepository
            .FindByIdAsync(attemptId, cancellationToken);

        if (attempt == null || attempt.UserId != userId)
            return Result.Failure<AnswerResponse>(Error.NotFound);

        if (attempt.Status != AttemptStatus.InProgress)
            return Result.Failure<AnswerResponse>(
                new Error("Attempt.NotInProgress", "This attempt is not in progress."));

        var question = await _unitOfWork.QuestionRepository
            .FindByIdAsync(request.QuestionId, cancellationToken, q => q.Section, q => q.Passage);

        if (question == null)
            return Result.Failure<AnswerResponse>(Error.NotFound);

        // Check if answer already exists
        var existingAnswer = await _unitOfWork.AnswerRepository
            .FindSingleAsync(a =>
                a.AttemptId == attemptId &&
                a.QuestionId == request.QuestionId,
                cancellationToken);

        Answer answer;
        if (existingAnswer != null)
        {
            // Update existing answer
            answer = existingAnswer;
        }
        else
        {
            // Create new answer
            answer = new Answer
            {
                AttemptId = attemptId,
                QuestionId = request.QuestionId
            };
            _unitOfWork.AnswerRepository.Add(answer);
        }

        // Handle different question types
        switch (question.Section.Type)
        {
            case SectionType.Writing:
                answer.EssayAnswer = request.EssayAnswer;
                answer.SelectedOptionId = null; // Explicitly set to null for writing

                // Queue for AI scoring if essay is provided
                if (!string.IsNullOrEmpty(request.EssayAnswer))
                {
                    await _scoringQueue.QueueScoringTaskAsync(new EssayScoringTask
                    {
                        AnswerId = answer.Id,
                        Essay = request.EssayAnswer,
                        Prompt = question.QuestionText ?? string.Empty
                    });
                }
                break;

            case SectionType.Reading:
            case SectionType.Listening:
                if (!request.SelectedOptionId.HasValue)
                {
                    return Result.Failure<AnswerResponse>(
                        new Error("Answer.OptionRequired", "Multiple choice answer requires a selected option."));
                }

                answer.SelectedOptionId = request.SelectedOptionId;
                answer.EssayAnswer = null; // Explicitly set to null for multiple choice

                // Calculate score immediately for multiple choice
                var selectedOption = await _unitOfWork.QuestionOptions
                    .FindByIdAsync(request.SelectedOptionId.Value, cancellationToken);
                answer.Score = selectedOption?.IsCorrect == true ? question.Points : 0;
                break;

            default:
                throw new NotSupportedException($"Question type {question.Section.Type} is not supported.");
        }

        if (existingAnswer != null)
        {
            _unitOfWork.AnswerRepository.Update(answer);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = _mapper.Map<AnswerResponse>(answer);
        return Result.Success(response);
    }

    public async Task<Result<AttemptResultResponse>> FinishAttemptAsync(
        int userId,
        FinishAttemptRequest request,
        CancellationToken cancellationToken = default)
    {
        var attempt = await _unitOfWork.StudentAttemptRepository
            .FindByIdAsync(request.AttemptId, cancellationToken);

        if (attempt == null || attempt.UserId != userId)
            return Result.Failure<AttemptResultResponse>(Error.NotFound);

        if (attempt.Status != AttemptStatus.InProgress)
            return Result.Failure<AttemptResultResponse>(
                new Error("Attempt.NotInProgress", "This attempt is not in progress."));

        attempt.EndTime = DateTime.UtcNow;
        attempt.Status = AttemptStatus.Completed;

        _unitOfWork.StudentAttemptRepository.Update(attempt);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetAttemptResultAsync(userId, request.AttemptId, cancellationToken);
    }

    public async Task<Result<AttemptResultResponse>> GetAttemptResultAsync(
        int userId,
        int attemptId,
        CancellationToken cancellationToken = default)
    {
        // Sử dụng repository method mới để load đầy đủ data
        var attempt = await _unitOfWork.StudentAttemptRepository
            .GetAttemptWithDetailsAsync(attemptId, cancellationToken);

        if (attempt == null || attempt.UserId != userId)
            return Result.Failure<AttemptResultResponse>(Error.NotFound);

        if (attempt.Status != AttemptStatus.Completed)
            return Result.Failure<AttemptResultResponse>(
                new Error("Attempt.NotCompleted", "This attempt is not completed."));

        // Calculate scores
        var result = new AttemptResultResponse
        {
            Id = attempt.Id,
            ExamTitle = attempt.Exam.Title!,
            StartTime = attempt.StartTime,
            EndTime = attempt.EndTime!.Value,
            Answers = _mapper.Map<List<AnswerResponse>>(attempt.Answers)
        };

        // Calculate total and section scores
        decimal totalScore = 0;
        decimal maximumScore = 0;
        var sectionScores = new Dictionary<string, decimal>();

        foreach (var section in attempt.Exam.Sections)
        {
            var sectionAnswers = attempt.Answers
                .Where(a => a.Question.SectionId == section.Id);

            var sectionScore = sectionAnswers.Sum(a => a.Score ?? 0);
            var sectionMaxScore = section.Questions.Sum(q => q.Points);

            sectionScores.Add(section.Title!, sectionScore);
            totalScore += sectionScore;
            maximumScore += sectionMaxScore;
        }

        result.TotalScore = totalScore;
        result.MaximumScore = maximumScore;
        result.Percentage = maximumScore > 0 ? (totalScore / maximumScore) * 100 : 0;
        result.SectionScores = sectionScores;

        return Result.Success(result);
    }
}
