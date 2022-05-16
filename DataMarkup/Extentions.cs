namespace DataMarkup;

public static class Extentions
{
    public static string CutIfMoreThan(this string text, int count) => text.Length < count ?
        text :
        $"{string.Concat(text.Take(count))}...";
}
