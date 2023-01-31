using System.Net;
using System.Text.Json;

namespace DataMarkup.Frontend;

public static class Extentions
{
    public static bool IsSuccesStatusCode(this HttpStatusCode statusCode) =>
        (int)statusCode >= 200 && (int)statusCode <= 299;

    public static async ValueTask<T?> ReadAsAsync<T>(this HttpContent content) where T : class
    {
        var contentString = await content.ReadAsStringAsync();

        var model = string.IsNullOrEmpty(contentString) ?
            null :
            JsonSerializer.Deserialize<T>(contentString);

        return model;
    }
}
