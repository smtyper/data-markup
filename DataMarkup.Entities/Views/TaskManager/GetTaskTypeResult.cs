namespace DataMarkup.Entities.Views.TaskManager;

public record GetTaskTypeResult : RequestResult
{
    public TaskType? TaskType { get; init; }
}
