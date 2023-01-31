using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using DataMarkup.Entities.Parameters.Account;
using DataMarkup.Entities.Views.Account;

namespace DataMarkup.Frontend;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly ApiAuthenticationStateProvider _apiAuthenticationStateProvider;
    private readonly ILocalStorageService _localStorage;

    public AuthService(HttpClient httpClient,
        ApiAuthenticationStateProvider apiAuthenticationStateProvider,
        ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _apiAuthenticationStateProvider = apiAuthenticationStateProvider;
        _localStorage = localStorage;
    }

    public async Task<RegisterResult?> Register(RegisterParameters registerModel)
    {
        const string uri = "Account/Register";

        using var response = await _httpClient.PostAsJsonAsync(uri, registerModel);
        var registerResult = await response.Content.ReadAsAsync<RegisterResult>();

        return registerResult;
    }

    public async Task<LoginResult?> Login(LoginParameters loginModel)
    {
        const string uri = "Account/Login";

        using var response = await _httpClient.PostAsJsonAsync(uri, loginModel);
        var contentString = await response.Content.ReadAsStringAsync();

        var loginResult = string.IsNullOrEmpty(contentString) ?
            null :
            JsonSerializer.Deserialize<LoginResult>(contentString);

        if (!response.IsSuccessStatusCode)
            return loginResult;

        await _localStorage.SetItemAsync("authToken", loginResult!.Token);
        _apiAuthenticationStateProvider.MarkUserAsAuthenticated(loginModel.Username!);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", loginResult.Token);

        return loginResult;
    }

    public async Task Logout()
    {
        await _localStorage.RemoveItemAsync("authToken");
        _apiAuthenticationStateProvider.MarkUserAsLoggedOut();
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }
}
