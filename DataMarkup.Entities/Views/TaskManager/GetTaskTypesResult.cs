namespace DataMarkup.Entities.Views.TaskManager;

public record GetTaskTypesResult : RequestResult
{
    public IReadOnlyCollection<TaskType> TaskTypes { get; init; } = null!;

    public int Count { get; init; }
}
