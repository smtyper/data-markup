using System.Net.Http.Json;
using System.Text.Json;
using DataMarkup.Entities.Parameters.Account;
using DataMarkup.Entities.Views.Account;

namespace DataMarkup.Frontend.Extensions;

public static class HttpClientExtensions
{
    public static async Task<RegisterResult?> RegisterAsync(this HttpClient httpClient, RegisterParameters registerParameters)
    {
        const string url = "Account/register";

        using var response = await httpClient.PostAsJsonAsync(url, registerParameters);

        var jsonContent = await response.Content.ReadAsStringAsync();
        var result = TryDeserialize<RegisterResult>(jsonContent);

        return result;
    }

    private static T? TryDeserialize<T>(string jsonValue) where T : class
    {
        try
        {
            var value = JsonSerializer.Deserialize<T>(jsonValue, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return value;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
