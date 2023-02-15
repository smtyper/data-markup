using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;

namespace DataMarkup.Frontend.Extensions;

public static class Extensions
{
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
