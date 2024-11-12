namespace VstepPractice.API.Models.Entities;

public interface IEntity<TKey>
{
    TKey Id { get; set; }
}