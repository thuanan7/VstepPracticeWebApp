using AutoMapper;
using VstepPractice.API.Common.Enums;
using VstepPractice.API.Common.Utils;
using VstepPractice.API.Models.DTOs.AI;
using VstepPractice.API.Models.DTOs.StudentAttempts.Requests;
using VstepPractice.API.Models.DTOs.StudentAttempts.Responses;
using VstepPractice.API.Models.Entities;
using VstepPractice.API.Repositories.Interfaces;
using VstepPractice.API.Services.AI;

namespace VstepPractice.API.Services.StudentAttempts;

public class StudentAttemptService : IStudentAttemptService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEssayScoringQueue _scoringQueue;
    private readonly ILogger<StudentAttemptService> _logger;

    public StudentAttemptService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IEssayScoringQueue scoringQueue,
        ILogger<StudentAttemptService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _scoringQueue = scoringQueue;
        _logger = logger;
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
            answer = existingAnswer;
        }
        else
        {
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
                answer.Score = null; // Reset score as it will be set by AI
                answer.AiFeedback = null; // Reset feedback as it will be set by AI
                break;

            case SectionType.Reading:
            case SectionType.Listening:
                if (!request.SelectedOptionId.HasValue)
                {
                    return Result.Failure<AnswerResponse>(
                        new Error("Answer.OptionRequired", "Multiple choice answer requires a selected option."));
                }

                answer.EssayAnswer = null; // Reset essay answer for multiple choice
                answer.SelectedOptionId = request.SelectedOptionId;
                answer.AiFeedback = null;

                // Calculate score immediately for multiple choice
                var selectedOption = await _unitOfWork.QuestionOptions
                    .FindByIdAsync(request.SelectedOptionId.Value, cancellationToken);
                answer.Score = selectedOption?.IsCorrect == true ? question.Points : 0;
                break;

            default:
                throw new NotSupportedException($"Question type {question.Section.Type} is not supported.");
        }

        // Save answer changes first
        if (existingAnswer != null)
        {
            _unitOfWork.AnswerRepository.Update(answer);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Queue writing assessment if needed
        if (question.Section.Type == SectionType.Writing && !string.IsNullOrEmpty(request.EssayAnswer))
        {
            await _scoringQueue.QueueScoringTaskAsync(new EssayScoringTask
            {
                AnswerId = answer.Id,
                PassageTitle = question.Passage.Title,
                PassageContent = question.Passage.Content ?? string.Empty,
                QuestionText = question.QuestionText ?? string.Empty,
                Essay = request.EssayAnswer
            });
        }

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
        var attempt = await _unitOfWork.StudentAttemptRepository
            .GetAttemptWithDetailsAsync(attemptId, cancellationToken);

        if (attempt == null || attempt.UserId != userId)
            return Result.Failure<AttemptResultResponse>(Error.NotFound);

        if (attempt.Status != AttemptStatus.Completed)
            return Result.Failure<AttemptResultResponse>(
                new Error("Attempt.NotCompleted", "This attempt is not completed."));

        _logger.LogInformation(
            "Processing attempt result. Sections: {SectionCount}, Answers: {AnswerCount}",
            attempt.Exam.Sections.Count,
            attempt.Answers.Count);

        // Map answers with writing assessments
        var answers = new List<AnswerResponse>();
        foreach (var answer in attempt.Answers)
        {
            var writingAssessment = answer.Question.Section.Type == SectionType.Writing
                ? await _unitOfWork.WritingAssessmentRepository
                    .GetByAnswerIdAsync(answer.Id, cancellationToken)
                : null;

            var answerResponse = _mapper.Map<AnswerResponse>(answer, opt =>
            {
                opt.Items["WritingAssessment"] = writingAssessment;
            });
            answers.Add(answerResponse);
        }

        var result = new AttemptResultResponse
        {
            Id = attempt.Id,
            ExamTitle = attempt.Exam.Title!,
            StartTime = attempt.StartTime,
            EndTime = attempt.EndTime!.Value,
            Answers = answers
        };

        // Calculate section scores
        var sectionScores = new Dictionary<string, decimal>();
        decimal listeningScore = 0;
        decimal readingScore = 0;
        decimal writingScore = 0;

        foreach (var section in attempt.Exam.Sections)
        {
            var sectionAnswers = attempt.Answers
                .Where(a => a.Question.SectionId == section.Id)
                .ToList();

            switch (section.Type)
            {
                case SectionType.Listening:
                    var correctListeningAnswers = sectionAnswers.Count(a =>
                        a.SelectedOptionId.HasValue &&
                        a.Question.Options.Any(o =>
                            o.Id == a.SelectedOptionId && o.IsCorrect));
                    listeningScore = VstepScoreCalculator.CalculateListeningScore(correctListeningAnswers);
                    sectionScores.Add("Listening", listeningScore);
                    break;

                case SectionType.Reading:
                    var readingParts = sectionAnswers
                        .GroupBy(a => a.Question.Part.PartNumber)
                        .Select(g => new
                        {
                            PartNumber = g.Key,
                            Score = g.Sum(a => a.Score ?? 0)
                        })
                        .ToList();

                    var partScores = readingParts.Select(p => p.Score).ToList();
                    readingScore = VstepScoreCalculator.CalculateReadingScore(partScores);
                    sectionScores.Add("Reading", readingScore);
                    break;

                case SectionType.Writing:
                    var writingParts = section.Parts
                        .OrderBy(p => p.PartNumber)
                        .ToList();

                    var writingDetails = new WritingSectionScore();

                    foreach (var part in writingParts)
                    {
                        var answer = sectionAnswers
                            .FirstOrDefault(a => a.Question.PartId == part.Id);

                        if (answer != null)
                        {
                            // Get writing assessment
                            var assessment = await _unitOfWork.WritingAssessmentRepository
                                .GetByAnswerIdAsync(answer.Id, cancellationToken);

                            if (assessment != null)
                            {
                                var taskScore = new WritingTaskScore
                                {
                                    TaskNumber = part.PartNumber,
                                    TaskAchievement = assessment.TaskAchievement,
                                    CoherenceCohesion = assessment.CoherenceCohesion,
                                    LexicalResource = assessment.LexicalResource,
                                    GrammarAccuracy = assessment.GrammarAccuracy
                                };

                                writingDetails.TaskScores.Add(taskScore);
                            }
                        }
                    }

                    // Only calculate if we have both tasks
                    if (writingDetails.TaskScores.Count == 2)
                    {
                        writingScore = writingDetails.FinalScore;
                        sectionScores.Add("Writing", writingScore);
                        result.WritingDetails = writingDetails;

                        _logger.LogInformation(
                            "Writing scores calculated. Task1: {Task1}, Task2: {Task2}, Final: {Final}",
                            writingDetails.TaskScores[0].TotalScore,
                            writingDetails.TaskScores[1].TotalScore,
                            writingScore);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Expected 2 writing tasks, but found {Count} for attempt {AttemptId}",
                            writingDetails.TaskScores.Count,
                            attemptId);
                    }
                    break;
            }
        }

        // Calculate final score
        var finalScore = VstepScoreCalculator.CalculateFinalScore(
            listeningScore, readingScore, writingScore);

        result.SectionScores = sectionScores;
        result.FinalScore = finalScore;

        return Result.Success(result);
    }
}
