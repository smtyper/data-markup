namespace DataMarkup.Models;

public record MarkupTask
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public int MaxSolutions { get; set; }

    public decimal Payment { get; set; }

    public string Instruction { get; set; }

    public List<MarkupQuestion> Questions { get; set; }

    public List<MarkupTaskInstance> Instances { get; set; }

    public User User { get; set; }
}
