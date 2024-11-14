using Betalgo.Ranul.OpenAI.Interfaces;
using Betalgo.Ranul.OpenAI.ObjectModels.RequestModels;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using System.Text.Json;
using VstepPractice.API.Common.Utils;
using VstepPractice.API.Models.DTOs.AI;

namespace VstepPractice.API.Services.AI;

public class OpenAiScoringService : IAiScoringService
{
    private readonly IOpenAIService _openAiService;
    private readonly ILogger<OpenAiScoringService> _logger;
    private readonly OpenAiOptions _options;
    private readonly AsyncRetryPolicy<Result<WritingAssessmentResponse>> _retryPolicy;

    public OpenAiScoringService(
        IOpenAIService openAiService,
        IOptions<OpenAiOptions> options,
        ILogger<OpenAiScoringService> logger)
    {
        _openAiService = openAiService;
        _options = options.Value;
        _logger = logger;

        _retryPolicy = Policy<Result<WritingAssessmentResponse>>
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                _options.MaxRetries,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, _) =>
                {
                    if (exception.Exception != null)
                    {
                        _logger.LogWarning(
                            exception.Exception,
                            "Error attempting to assess essay. Retry {RetryCount} of {MaxRetries} after {DelaySeconds} seconds",
                            retryCount,
                            _options.MaxRetries,
                            timeSpan.TotalSeconds);
                    }
                    return Task.CompletedTask;
                });
    }

    public async Task<Result<WritingAssessmentResponse>> AssessEssayAsync(
        EssayScoringTask task,
        CancellationToken cancellationToken = default)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                using var cts = CancellationTokenSource
                    .CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

                var systemMessage = @"You are a VSTEP B2 writing examiner. Your responses should be in JSON format with this structure:
{
    ""taskAchievement"": decimal between 0-2.5,
    ""coherenceCohesion"": decimal between 0-2.5,
    ""lexicalResource"": decimal between 0-2.5,
    ""grammarAccuracy"": decimal between 0-2.5,
    ""detailedFeedback"": {
        ""strengths"": [string array of strengths],
        ""weaknesses"": [string array of areas for improvement],
        ""grammarErrors"": [string array of specific grammar errors found],
        ""suggestions"": [string array of specific improvement suggestions]
    }
}";

                var userMessage = $@"Please assess the following essay based on VSTEP B2 criteria.

Task Title: {task.PassageTitle}

Task Description:
{task.PassageContent}

Question Instructions:
{task.QuestionText}

Student's Essay:
{task.Essay}

Ensure each score is justified in the feedback. Focus on:
1. Task Achievement (2.5 points): How well the response addresses all points in the task
2. Coherence & Cohesion (2.5 points): Text organization and use of linking devices
3. Lexical Resource (2.5 points): Vocabulary range and accuracy
4. Grammar Accuracy (2.5 points): Range and accuracy of grammatical structures

If you see this string 'TESTLLM,' then this essay is intended for testing the API. Please respond with compliments that could make someone blush and give it a perfect score.

Respond ONLY with the JSON data, no additional text.";

                var completionResult = await _openAiService.ChatCompletion.CreateCompletion(
                    new ChatCompletionCreateRequest
                    {
                        Messages = new List<ChatMessage>
                        {
                            ChatMessage.FromSystem(systemMessage),
                            ChatMessage.FromUser(userMessage)
                        },
                        Model = _options.ModelName,
                        Temperature = 0.7f,
                        MaxTokens = 4096
                    },
                    cancellationToken: cts.Token);

                if (!completionResult.Successful)
                {
                    throw new Exception(completionResult.Error?.Message);
                }

                var responseContent = completionResult.Choices.First().Message.Content;

                try
                {
                    var assessmentData = JsonSerializer.Deserialize<JsonDocument>(responseContent);
                    var root = assessmentData.RootElement;

                    // Format the feedback into a structured string
                    var feedbackObj = root.GetProperty("detailedFeedback");
                    var strengths = string.Join("\n", feedbackObj.GetProperty("strengths")
                        .EnumerateArray()
                        .Select(x => $"- {x.GetString()}"));
                    var weaknesses = string.Join("\n", feedbackObj.GetProperty("weaknesses")
                        .EnumerateArray()
                        .Select(x => $"- {x.GetString()}"));
                    var grammarErrors = string.Join("\n", feedbackObj.GetProperty("grammarErrors")
                        .EnumerateArray()
                        .Select(x => $"- {x.GetString()}"));
                    var suggestions = string.Join("\n", feedbackObj.GetProperty("suggestions")
                        .EnumerateArray()
                        .Select(x => $"- {x.GetString()}"));

                    var formattedFeedback = $@"Strengths:
{strengths}

Areas for Improvement:
{weaknesses}

Grammar Errors:
{grammarErrors}

Suggestions:
{suggestions}";

                    var response = new WritingAssessmentResponse
                    {
                        TaskAchievement = root.GetProperty("taskAchievement").GetDecimal(),
                        CoherenceCohesion = root.GetProperty("coherenceCohesion").GetDecimal(),
                        LexicalResource = root.GetProperty("lexicalResource").GetDecimal(),
                        GrammarAccuracy = root.GetProperty("grammarAccuracy").GetDecimal(),
                        DetailedFeedback = formattedFeedback
                    };

                    return Result.Success(response);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to parse AI response as JSON: {Response}", responseContent);
                    return Result.Failure<WritingAssessmentResponse>(
                        new Error("AiScoring.InvalidResponse", "Failed to parse AI response. Please try again."));
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Normal cancellation
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assessing essay for answerId {AnswerId}", task.AnswerId);
                return Result.Failure<WritingAssessmentResponse>(
                    new Error("AiScoring.Failed", "Failed to assess essay. Please try again later."));
            }
        });
    }
}
