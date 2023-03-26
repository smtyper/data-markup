namespace DataMarkup.Entities.Views.TaskManager;

public record GetTaskInstancesResult : RequestResult
{
    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public int TotalPageCount { get; init; }

    public int FullySolvedTasksCont { get; init; }

    public IReadOnlyCollection<Task> Tasks { get; init; } = null!;
}
