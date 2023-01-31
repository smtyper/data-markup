namespace DataMarkup.Entities.Views.Board;

public record GetTaskResult : RequestResult
{
    public Task? Task { get; init; }
}
