namespace DataMarkup.Entities.Views.Board;

public record GetTaskResult : RequestResult
{
    public SolverTask? Task { get; init; }
}
