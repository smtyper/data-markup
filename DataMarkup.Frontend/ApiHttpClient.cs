using System.Net.Http.Json;
using System.Text.Json;
using Blazored.SessionStorage;
using DataMarkup.Entities.Parameters.Account;
using DataMarkup.Entities.Views.Account;
using Microsoft.AspNetCore.Components.Authorization;

namespace DataMarkup.Frontend;

public class ApiHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ISessionStorageService _sessionStorage;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public ApiHttpClient(HttpClient httpClient, ISessionStorageService sessionStorage,
        AuthenticationStateProvider authenticationStateProvider)
    {
        _httpClient = httpClient;
        _sessionStorage = sessionStorage;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async ValueTask<LoginResult?> LoginAsync(LoginParameters loginParameters)
    {
        const string url = "Account/login";

        using var response = await _httpClient.PostAsJsonAsync(url, loginParameters);

        var jsonContent = await response.Content.ReadAsStringAsync();
        var result = TryDeserialize<LoginResult>(jsonContent);

        if (result?.Successful is true)
            _httpClient.DefaultRequestHeaders.Add("bearer", result.Token);

        return result;
    }

    public async ValueTask<RegisterResult?> RegisterAsync(RegisterParameters registerParameters)
    {
        const string url = "Account/register";

        using var response = await _httpClient.PostAsJsonAsync(url, registerParameters);

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
