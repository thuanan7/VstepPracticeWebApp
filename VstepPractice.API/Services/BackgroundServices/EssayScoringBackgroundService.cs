using System.Threading.Channels;
using VstepPractice.API.Models.Entities;
using VstepPractice.API.Repositories.Interfaces;
using VstepPractice.API.Services.AI;

namespace VstepPractice.API.Services.BackgroundServices;

public class EssayScoringTask
{
    public int AnswerId { get; set; }
    public string Essay { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
}

public class EssayScoringBackgroundService : BackgroundService, IEssayScoringQueue
{
    private readonly Channel<EssayScoringTask> _taskChannel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EssayScoringBackgroundService> _logger;

    public EssayScoringBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<EssayScoringBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        // Create unbounded channel
        _taskChannel = Channel.CreateUnbounded<EssayScoringTask>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }

    public async Task QueueScoringTaskAsync(EssayScoringTask task)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        await _taskChannel.Writer.WriteAsync(task);
        _logger.LogInformation("Queued scoring task for answerId {AnswerId}", task.AnswerId);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Wait for task from channel
                var task = await _taskChannel.Reader.ReadAsync(stoppingToken);
                await ProcessScoringTaskAsync(task, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Normal shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing essay scoring task");
            }
        }
    }

    private async Task ProcessScoringTaskAsync(EssayScoringTask task, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var aiScoringService = scope.ServiceProvider.GetRequiredService<IAiScoringService>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // Check if assessment already exists
            var existingAssessment = await unitOfWork.WritingAssessmentRepository
                .GetByAnswerIdAsync(task.AnswerId, cancellationToken);

            if (existingAssessment != null)
            {
                _logger.LogInformation(
                    "Assessment already exists for answerId {AnswerId}", task.AnswerId);
                return;
            }

            // Get AI assessment
            var assessmentResult = await aiScoringService.AssessEssayAsync(
                task.AnswerId,
                task.Essay,
                task.Prompt,
                cancellationToken);

            if (!assessmentResult.IsSuccess)
            {
                _logger.LogError(
                    "Failed to get AI assessment for answerId {AnswerId}: {Error}",
                    task.AnswerId,
                    assessmentResult.Error.Message);
                return;
            }

            var assessment = assessmentResult.Value;

            // Create WritingAssessment
            var writingAssessment = new WritingAssessment
            {
                AnswerId = task.AnswerId,
                TaskAchievement = assessment.TaskAchievement,
                CoherenceCohesion = assessment.CoherenceCohesion,
                LexicalResource = assessment.LexicalResource,
                GrammarAccuracy = assessment.GrammarAccuracy,
                DetailedFeedback = assessment.DetailedFeedback
            };

            // Update Answer
            var answer = await unitOfWork.AnswerRepository
                .FindByIdAsync(task.AnswerId, cancellationToken);

            if (answer != null)
            {
                answer.Score = assessment.TotalScore;
                answer.AiFeedback = assessment.DetailedFeedback;
                unitOfWork.AnswerRepository.Update(answer);
            }

            // Save assessment
            unitOfWork.WritingAssessmentRepository.Add(writingAssessment);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully processed assessment for answerId {AnswerId}", task.AnswerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing assessment for answerId {AnswerId}", task.AnswerId);
        }
    }
}
