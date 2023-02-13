using System.Text;
using System.Text.Json;
using Blazored.SessionStorage;

namespace DataMarkup.Frontend.Extensions;

public static class Extensions
{
    public static string CutIfMoreThan(this string text, int count) => text.Length < count ?
        text :
        $"{string.Concat(text.Take(count))}...";

    public static async ValueTask AddAsBase64Async<T>(this ISessionStorageService sessionStorageService, string key,
        T value, CancellationToken cancellationToken = default)
    {
        var jsonValue = JsonSerializer.Serialize(value);
        var jsonBytes = Encoding.UTF8.GetBytes(jsonValue);
        var base64JsonValue = Convert.ToBase64String(jsonBytes);

        await sessionStorageService.SetItemAsync(key, base64JsonValue, cancellationToken);
    }

    public static async ValueTask<T?> GetBase64ValueAsync<T>(this ISessionStorageService sessionStorageService,
        string key, CancellationToken cancellationToken = default)
    {
        var base64JsonValue = await sessionStorageService.GetItemAsync<string>(key, cancellationToken);
        var base64Bytes = Convert.FromBase64String(base64JsonValue);
        var jsonString = Encoding.UTF8.GetString(base64Bytes);
        var value = JsonSerializer.Deserialize<T>(jsonString);

        return value;
    }
}
