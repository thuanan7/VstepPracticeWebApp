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
        int answerId,
        string essay,
        string prompt,
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

                var userMessage = $@"Please assess the following essay based on VSTEP criteria.
Original task: {prompt}

Essay to assess:
{essay}

Ensure each score is justified in the feedback. Focus on actionable feedback that helps the student improve. Respond ONLY with the JSON data, no additional text.";

                var completionResult = await _openAiService.ChatCompletion.CreateCompletion(
                    new ChatCompletionCreateRequest
                    {
                        Messages = new List<ChatMessage>
                        {
                            ChatMessage.FromSystem(systemMessage),
                            ChatMessage.FromUser(userMessage)
                        },
                        Model = Betalgo.Ranul.OpenAI.ObjectModels.Models.Gpt_4,
                        Temperature = 0.7f,
                        MaxTokens = 1000
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
                _logger.LogError(ex, "Error assessing essay for answerId {AnswerId}", answerId);
                return Result.Failure<WritingAssessmentResponse>(
                    new Error("AiScoring.Failed", "Failed to assess essay. Please try again later."));
            }
        });
    }
}

public class OpenAiOptions
{
    public const string SectionName = "OpenAI";

    public string ApiKey { get; set; } = "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";
    public string ModelName { get; set; } = "gpt-4";
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
}
