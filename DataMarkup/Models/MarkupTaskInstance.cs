namespace DataMarkup.Models;

public record MarkupTaskInstance
{
    public Guid Id { get; set; }

    public MarkupTask Task { get; set; }

    public List<MarkupQuestionInstance> QuestionInstances { get; set; }
}

public record MarkupQuestionInstance
{
    public Guid Id { get; set; }

    public string Content { get; set; }

    public MarkupQuestion Question { get; set; }

    public MarkupTaskInstance TaskInstance { get; set; }
}
