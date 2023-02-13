namespace DataMarkup.Frontend.Models;

public record Card
{
    public string Title { get; init;} = null!;

    public string Subtitle { get; init;} = null!;

    public string TextArea { get; init;} = null!;

    public string ButtonText { get; init;} = null!;
}
