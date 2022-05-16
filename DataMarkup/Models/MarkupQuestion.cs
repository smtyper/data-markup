namespace DataMarkup.Models;

public record MarkupQuestion
{
    public Guid Id { get; set; }

    public string StaticContent { get; set; }

    public string DynamicContentConstraint { get; set; }

    public string AnswerDescription { get; set; }

    public string AnswerConstraint { get; set; }

    public bool ContainsImage { get; set; }

    public MarkupTask Task { get; set; }
}
