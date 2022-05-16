namespace DataMarkup.Models;

public record MarkupTask
{
    public Guid Id { get; init; }

    public string Name { get; init; }

    public int MaxSolutions { get; init; }

    public decimal Payment { get; init; }

    public string Instruction { get; init; }

    public List<MarkupQuestion> MarkupQuestions { get; init; }

    public User User { get; init; }
}
