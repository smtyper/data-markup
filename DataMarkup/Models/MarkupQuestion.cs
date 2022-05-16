using System.ComponentModel.DataAnnotations;

namespace DataMarkup.Models;

public record MarkupQuestion
{
    public Guid Id { get; init; }

    public string StaticContent { get; init; }

    public string DynamicContentConstraint { get; init; }

    public string AnswerDescription { get; init; }

    public string AnswerConstraint { get; init; }

    public bool ContainsImage { get; init; }

    public MarkupTask Task { get; init; }
}
