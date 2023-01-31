namespace DataMarkup.Entities.Views.Board;

public record GetAvailableTaskTypesResult : RequestResult
{
    public IReadOnlyCollection<TaskType> TaskTypes { get; init; } = null!;

    public int Count { get; init; }
}
