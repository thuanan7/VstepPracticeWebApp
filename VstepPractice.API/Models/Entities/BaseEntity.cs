namespace VstepPractice.API.Models.Entities;

public abstract class BaseEntity : IEntity<int>
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
