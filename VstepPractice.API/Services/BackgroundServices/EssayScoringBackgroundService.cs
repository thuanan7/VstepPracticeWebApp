using System.Threading.Channels;
using VstepPractice.API.Models.DTOs.AI;
using VstepPractice.API.Models.Entities;
using VstepPractice.API.Repositories.Interfaces;
using VstepPractice.API.Services.AI;

namespace VstepPractice.API.Services.BackgroundServices;

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

            // Start transaction
            await unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                _logger.LogInformation(
                    "Starting to process assessment for answerId {AnswerId}", task.AnswerId);
                // Get answer with tracking
                var answer = await unitOfWork.AnswerRepository
                    .FindByIdAsync(task.AnswerId, cancellationToken);

                if (answer == null)
                {
                    _logger.LogError("Answer not found for answerId {AnswerId}", task.AnswerId);
                    return;
                }

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
                    task,
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
                    DetailedFeedback = assessment.DetailedFeedback,
                    AssessedAt = DateTime.UtcNow
                };

                // Update Answer
                answer.Score = assessment.TotalScore;
                answer.AiFeedback = assessment.DetailedFeedback;

                // Log the assessment details before saving
                _logger.LogDebug(
                    "Assessment details for answerId {AnswerId}: TaskAchievement={TaskAchievement}, " +
                    "CoherenceCohesion={CoherenceCohesion}, LexicalResource={LexicalResource}, " +
                    "GrammarAccuracy={GrammarAccuracy}",
                    task.AnswerId,
                    writingAssessment.TaskAchievement,
                    writingAssessment.CoherenceCohesion,
                    writingAssessment.LexicalResource,
                    writingAssessment.GrammarAccuracy);

                // Log before each major operation
                _logger.LogDebug("Updating Answer entity");
                unitOfWork.AnswerRepository.Update(answer);

                _logger.LogDebug("Adding WritingAssessment entity");
                unitOfWork.WritingAssessmentRepository.Add(writingAssessment);

                _logger.LogDebug("Saving changes to database");
                await unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogDebug("Committing transaction");
                await unitOfWork.CommitAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully processed assessment for answerId {AnswerId}", task.AnswerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Detailed error for answerId {AnswerId}: {ErrorMessage}",
                    task.AnswerId,
                    ex.ToString());
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing assessment for answerId {AnswerId}", task.AnswerId);
        }
    }
}
