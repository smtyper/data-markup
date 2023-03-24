using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using ChartJs.Blazor.Common;
using ChartJs.Blazor.PieChart;

namespace DataMarkup.Frontend;

public static class Extensions
{
    public static PieConfig GetPieConfig<T>(IEnumerable<T> datasetValues, string? title = null,
        IEnumerable<string>? labels = null, IndexableOption<string>? colors = null)
    {
        var pieConfig = new PieConfig
        {
            Options = new PieOptions
            {
                Responsive = true,
                Title = title is null ?
                    null :
                    new OptionsTitle
                    {
                        Display = true,
                        Text = title
                    },
            }
        };

        foreach (var label in labels ?? Enumerable.Empty<string>())
            pieConfig.Data.Labels.Add(label);

        var dataset = new PieDataset<T>(datasetValues)
        {
            BackgroundColor = colors
        };

        pieConfig.Data.Datasets.Add(dataset);

        return pieConfig;
    }

    public static IndexableOption<string> GetBluePieColors() => new []
    {
        "#33567f",
        "#3a6190",
        "#416c9f",
        "#4774ab",
        "#4c7db7",
        "#668dc2",
        "#8aa3cc",
        "#a3b5d4",
        "#b9c6dd",
        "#ccd5e6",
    };

    public static IReadOnlyList<T> AsReadOnlyList<T>(this IReadOnlyCollection<T> collection) =>
        (IReadOnlyList<T>)collection;

    public static IEnumerable<IEnumerable<(T Value, int Index, int ColumnWidth)>> SepareteIntoRows<T>(
        this IEnumerable<T> items, int itemsInRow) => items
        .Select((item, index) => (item, index))
        .GroupBy(pair => pair.index / itemsInRow)
        .Select(group => group
            .Select(item => (item.item, item.index, 12 / itemsInRow)));

    public static string CutIfMoreThan(this string text, int count) => text.Length < count ?
        text :
        $"{string.Concat(text.Take(count))}...";

    public static async ValueTask AddAsBase64Async<T>(this ILocalStorageService localStorageService, string key,
        T value, CancellationToken cancellationToken = default)
    {
        var jsonValue = JsonSerializer.Serialize(value);
        var jsonBytes = Encoding.UTF8.GetBytes(jsonValue);
        var base64JsonValue = Convert.ToBase64String(jsonBytes);

        await localStorageService.SetItemAsync(key, base64JsonValue, cancellationToken);
    }

    public static async ValueTask<T?> GetBase64ValueAsync<T>(this ILocalStorageService localStorageService,
        string key, CancellationToken cancellationToken = default)
    {
        var base64JsonValue = await localStorageService.GetItemAsync<string>(key, cancellationToken);
        var base64Bytes = Convert.FromBase64String(base64JsonValue);
        var jsonString = Encoding.UTF8.GetString(base64Bytes);
        var value = JsonSerializer.Deserialize<T>(jsonString);

        return value;
    }
}
