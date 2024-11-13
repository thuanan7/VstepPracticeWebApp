using VstepPractice.API.Services.BackgroundServices;

namespace VstepPractice.API.Services.AI;

public interface IEssayScoringQueue
{
    Task QueueScoringTaskAsync(EssayScoringTask task);
}
